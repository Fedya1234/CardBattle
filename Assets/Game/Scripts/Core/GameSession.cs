using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Core
{
    public class GameSession
    {
        private const int PlayersCount = 2;
        private const int PlayerIndex = 0;
        private const int StartCardsCount = 4;
        private List<BaseMoveController> _moveControllers = new ();
        private GameState _gameState;
        private int _currentRound;
        
        public GameSession(PlayerSave[] players)
        {
            _currentRound = 0;
            _gameState = new GameState(players, PlayerIndex);
            
            _moveControllers.Add(new PlayerMoveController(0));
            _moveControllers.Add(new BotMoveController(1));
            
        }

        public async UniTaskVoid ChooseCards(List<CardLevel> cards)
        {
            List<UniTask<ChooseCardsReplyData>> chooseCards = new List<UniTask<ChooseCardsReplyData>>();
            for (int i = 0; i < PlayersCount; i++)
            {
                var moveController = _moveControllers[i];
                var startCards = _gameState.GetState(i).Cards.GetCardsFromDeck(StartCardsCount);
                chooseCards.Add(moveController.ChooseCards(startCards));
            }
            
            await UniTask.WhenAll(chooseCards);
            
            foreach (var task in chooseCards)
            {
                var reply = await task;
                OnPlayerCardsChosen(reply); // I Stop here
            }
        }
        
        private void DoTurn()
        {
            _currentRound++;
            
            foreach (var moveController in _moveControllers)
            {
                moveController.MakeMove();
            }
        }

        private void OnPlayerMove(PlayerMove move, int index)
        {
            
        }

        private void OnPlayerCardsChosen(ChooseCardsReplyData reply)
        {
            var index = reply.Index;
            var cards = reply.Hand;
            
            var handCards = new List<CardLevel>(cards);
            var needMoreCards = StartCardsCount - cards.Count;
            if (needMoreCards > 0)
            {
                var cardsFromDeck = _gameState.GetState(index).Cards.GetCardsFromDeck(needMoreCards);
                handCards.AddRange(cardsFromDeck);
            }
            //_gameState.GetState(index).Cards.GetCardsFromDeck()
            
        }
    }
}