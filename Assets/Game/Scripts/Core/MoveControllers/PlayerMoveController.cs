using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Core.MoveControllers
{
    public class PlayerMoveController : BaseMoveController
    {
        private PlayerMove _pendingMove;
        private List<CardLevel> _pendingCardChoices;
        private bool _moveReady;
        private bool _cardChoicesReady;
        
        // We'll simulate player input being "ready" after a delay for testing purposes
        private const int SimulatedDecisionDelay = 1000; // milliseconds
        
        public PlayerMoveController(int index) : base(index)
        {
            ResetState();
        }
        
        private void ResetState()
        {
            _pendingMove = new PlayerMove();
            _pendingCardChoices = new List<CardLevel>();
            _moveReady = false;
            _cardChoicesReady = false;
        }
        
        public override async UniTask<PlayerMoveReplyData> MakeMove()
        {
            ResetState();
            
            // In a real implementation, this would wait for UI input
            // For testing, we'll just delay and return a simulated move
            await UniTask.Delay(SimulatedDecisionDelay);
            
            // Simulated player move (random card to random position)
            // In a real implementation, this would come from the UI
            _pendingMove = GenerateRandomMove();
            _moveReady = true;
            
            return new PlayerMoveReplyData(_pendingMove, Index);
        }

        public override async UniTask<ChooseCardsReplyData> ChooseCards(List<CardLevel> cards)
        {
            // In a real implementation, this would wait for UI input
            // For testing, we'll just delay and return a simulated choice
            await UniTask.Delay(SimulatedDecisionDelay);
            
            // For the test implementation, keep all cards
            _pendingCardChoices = new List<CardLevel>(cards);
            _cardChoicesReady = true;
            
            return new ChooseCardsReplyData(_pendingCardChoices, new List<CardLevel>(), Index);
        }
        
        /// <summary>
        /// Generates a random move for testing purposes
        /// </summary>
        private PlayerMove GenerateRandomMove()
        {
            var move = new PlayerMove();
            
            // Simulating playing 1-2 cards
            int cardsToPlay = Random.Range(1, 3);
            
            for (int i = 0; i < cardsToPlay; i++)
            {
                var cardMove = new CardMove(
                    new CardLevel(
                        (CardId)Random.Range(0, 3),  // Random card ID (0-2)
                        1),                         // Level 1
                    Random.Range(0, 3),              // Random line (0-2)
                    Random.Range(0, 3),              // Random row (0-2)
                    Index                            // Player index
                );
                
                move.AddCard(cardMove);
            }
            
            return move;
        }
        
        // Methods for UI integration in a real implementation
        
        /// <summary>
        /// Sets the player's move (would be called by UI)
        /// </summary>
        public void SetPlayerMove(PlayerMove move)
        {
            _pendingMove = move;
            _moveReady = true;
        }
        
        /// <summary>
        /// Sets the player's card choices (would be called by UI)
        /// </summary>
        public void SetCardChoices(List<CardLevel> chosenCards, List<CardLevel> discardedCards)
        {
            _pendingCardChoices = chosenCards;
            _cardChoicesReady = true;
        }
    }
}