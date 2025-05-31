using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Core.MoveControllers;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Core
{
    public class GameSession
    {
        public const int PlayerIndex = 0;
        private const int PlayersCount = 2;
        private const int StartCardsCount = 4;
        private List<BaseMoveController> _moveControllers = new ();
        private GameState _gameState;
        private int _currentRound;
        private BattleResolver _battleResolver;
        private bool _gameEnded = false;
        
        // Events that UI can subscribe to
        public delegate void GameStateUpdatedHandler(GameState state);
        public event GameStateUpdatedHandler OnGameStateUpdated;
        
        public delegate void GameEndedHandler(int winnerIndex);
        public event GameEndedHandler OnGameEnded;
        
        public GameSession(PlayerSave[] players)
        {
            _currentRound = 0;
            _gameState = new GameState(players, PlayerIndex);
            
            _moveControllers.Add(new PlayerMoveController(0));
            _moveControllers.Add(new BotMoveController(1));
            
            _battleResolver = new BattleResolver();
        }

        /// <summary>
        /// Gets the PlayerMoveController for UI integration
        /// </summary>
        public PlayerMoveController GetPlayerMoveController()
        {
            return _moveControllers[PlayerIndex] as PlayerMoveController;
        }
        
        /// <summary>
        /// Gets the current game state
        /// </summary>
        public GameState GetGameState()
        {
            return _gameState;
        }

        /// <summary>
        /// Starts the game by handling the initial card draw
        /// </summary>
        public async UniTask StartGame()
        {
            // Initialize each player with their starting mana (2, as per the requirements)
            foreach (var player in _gameState.Players)
            {
                player.Hero.ChangeMana(2);
            }
            
            // Draw initial cards for each player
            List<UniTask<ChooseCardsReplyData>> chooseCards = new List<UniTask<ChooseCardsReplyData>>();
            for (int i = 0; i < PlayersCount; i++)
            {
                var moveController = _moveControllers[i];
                var startCards = _gameState.GetState(i).Cards.GetCardsFromDeck(StartCardsCount);
                chooseCards.Add(moveController.ChooseCards(startCards));
            }
            
            // Store the results in a variable to avoid awaiting twice
            var results = await UniTask.WhenAll(chooseCards);
            
            // Process the results directly
            foreach (var reply in results)
            {
                OnPlayerCardsChosen(reply);
            }
            
            // Notify subscribers that the game state has been updated
            OnGameStateUpdated?.Invoke(_gameState);
            
            // Start the first turn
            await DoTurn();
        }
        
        /// <summary>
        /// Executes a full turn of the game
        /// </summary>
        private async UniTask DoTurn()
        {
            if (_gameEnded)
                return;
            
            _currentRound++;
            Debug.Log($"Starting round {_currentRound}");
            
            // Increment mana for each player (up to max 9)
            foreach (var player in _gameState.Players)
            {
                if (player.Hero.Mana < 9)
                {
                    player.Hero.ChangeMana(1);
                }
            }
            
            // Each player draws a card at the start of their turn
            for (int i = 0; i < PlayersCount; i++)
            {
                _gameState.GetState(i).Cards.TakeCard();
            }
            
            // Notify subscribers that the game state has been updated (mana increased, cards drawn)
            OnGameStateUpdated?.Invoke(_gameState);
            
            // Share game state with bot controller for decision making
            if (_moveControllers[1] is BotMoveController botController)
            {
                botController.SetGameState(_gameState);
            }
            
            // Get moves from both players simultaneously
            var player1MoveTask = _moveControllers[0].MakeMove();
            var player2MoveTask = _moveControllers[1].MakeMove();
            
            // Store the results instead of awaiting twice
            var results = await UniTask.WhenAll(player1MoveTask, player2MoveTask);
            
            // Access the results from the array returned by WhenAll
            var player1Move = results.Item1;
            var player2Move = results.Item2;
            
            // Apply the moves to the game state
            OnPlayerMove(player1Move.Move, player1Move.Index);
            OnPlayerMove(player2Move.Move, player2Move.Index);
            
            // Notify subscribers that moves have been made
            OnGameStateUpdated?.Invoke(_gameState);
            
            // Resolve the battle
            bool gameOver = await _battleResolver.ResolveBattle(_gameState, player1Move.Move, player2Move.Move);
            
            // Notify subscribers that the battle has been resolved
            OnGameStateUpdated?.Invoke(_gameState);
            
            if (gameOver)
            {
                _gameEnded = true;
                // Determine the winner
                if (_gameState.GetState(0).Hero.Health <= 0 && _gameState.GetState(1).Hero.Health <= 0)
                {
                    // It's a draw
                    OnGameEnded?.Invoke(-1);
                }
                else if (_gameState.GetState(0).Hero.Health <= 0)
                {
                    // Player 2 wins
                    OnGameEnded?.Invoke(1);
                }
                else
                {
                    // Player 1 wins
                    OnGameEnded?.Invoke(0);
                }
            }
            else
            {
                // Start the next turn
                await DoTurn();
            }
        }

        /// <summary>
        /// Processes a player's move by applying card effects
        /// </summary>
        private void OnPlayerMove(PlayerMove move, int index)
        {
            // Apply the move to the player's state
            _gameState.GetState(index).ApplyChanges(move);
        }

        /// <summary>
        /// Handles the initial card selection phase
        /// </summary>
        private void OnPlayerCardsChosen(ChooseCardsReplyData reply)
        {
            var index = reply.Index;
            var cards = reply.Hand;
            var discard = reply.Discard;
            
            // Return discarded cards to the deck
            _gameState.GetState(index).Cards.AddCardsToDeck(discard);
            
            // Add the chosen cards to the player's hand
            var handCards = new List<CardLevel>(cards);
            var needMoreCards = StartCardsCount - cards.Count;
            if (needMoreCards > 0)
            {
                // If player kept fewer than the starting amount, draw more
                var cardsFromDeck = _gameState.GetState(index).Cards.GetCardsFromDeck(needMoreCards);
                handCards.AddRange(cardsFromDeck);
            }
            
            // Set the player's hand
            _gameState.GetState(index).Cards.HandCards = handCards;
        }
        
        /// <summary>
        /// Burns a card to gain mana (UI will call this method)
        /// </summary>
        /// <param name="cardIndex">Index of the card in the player's hand</param>
        /// <returns>True if successful</returns>
        public bool BurnCardForMana(int cardIndex)
        {
            var playerState = _gameState.GetMyState();
            
            if (cardIndex < 0 || cardIndex >= playerState.Cards.HandCards.Count)
            {
                Debug.LogWarning("Invalid card index for burning");
                return false;
            }
            
            // Remove the card from hand
            var card = playerState.Cards.HandCards[cardIndex];
            playerState.Cards.HandCards.RemoveAt(cardIndex);
            
            // Add to discard pile
            playerState.Cards.Discard.Add(card);
            
            // Add mana
            playerState.Hero.ChangeMana(1);
            
            // Notify subscribers that the game state has been updated
            OnGameStateUpdated?.Invoke(_gameState);
            
            return true;
        }
        
        /// <summary>
        /// Ends the turn (UI will call this method)
        /// </summary>
        public void EndTurn()
        {
            // This is a no-op for now, as the turn system is already handled in DoTurn()
            // If we want to add additional end-of-turn logic, it would go here
        }
    }
}
