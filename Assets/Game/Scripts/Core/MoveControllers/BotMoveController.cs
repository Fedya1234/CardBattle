using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Core.MoveControllers
{
    public class BotMoveController : BaseMoveController
    {
        private const int ThinkingDelay = 500; // milliseconds, simulates "thinking"
        private GameState _lastKnownGameState;
        private float _lastAdvantageScore;
        private BattleResolver _battleResolver;

        public BotMoveController(int index) : base(index)
        {
            _battleResolver = new BattleResolver();
            _lastAdvantageScore = 0;
        }

        public override async UniTask<PlayerMoveReplyData> MakeMove()
        {
            // Simulate AI thinking time
            await UniTask.Delay(ThinkingDelay);

            // Generate a strategic move
            var move = GenerateMove();

            // Choose an appropriate emotion based on game state
            EmotionType emotion = DetermineEmotion();

            return new PlayerMoveReplyData(move, Index, emotion);
        }

        public override async UniTask<ChooseCardsReplyData> ChooseCards(List<CardLevel> cards)
        {
            // Simulate AI thinking time
            await UniTask.Delay(ThinkingDelay);

            // Simple AI logic - keep the lowest cost cards
            // In a real implementation, this would use more sophisticated logic
            var sortedCards = cards.OrderBy(_ => Random.value).ToList();

            // Keep 60-80% of cards, discard the rest
            int cardsToKeep = Mathf.FloorToInt(cards.Count * Random.Range(0.6f, 0.8f));
            cardsToKeep = Mathf.Max(1, cardsToKeep); // Keep at least 1 card

            var keep = sortedCards.Take(cardsToKeep).ToList();
            var discard = sortedCards.Skip(cardsToKeep).ToList();

            return new ChooseCardsReplyData(keep, discard, Index);
        }

        /// <summary>
        /// Sets the current game state for AI decision making
        /// </summary>
        public void SetGameState(GameState gameState)
        {
            _lastKnownGameState = gameState;

            // Calculate advantage score if we have game state
            if (_lastKnownGameState != null)
            {
                // Negate the advantage score since positive means player 1 advantage
                // and we want positive to mean bot advantage
                _lastAdvantageScore = -_battleResolver.AnalyzeGameState(_lastKnownGameState);
            }
        }

        /// <summary>
        /// Determines the bot's emotional reaction based on game state
        /// </summary>
        private EmotionType DetermineEmotion()
        {
            // If we don't have game state info, default to None
            if (_lastKnownGameState == null)
                return EmotionType.None;

            // Base emotion on advantage score
            if (_lastAdvantageScore > 0.5f)
                return EmotionType.Happy; // Bot has big advantage
            else if (_lastAdvantageScore > 0.2f)
                return EmotionType.Confident; // Bot has slight advantage
            else if (_lastAdvantageScore < -0.5f)
                return EmotionType.Sad; // Bot is losing badly
            else if (_lastAdvantageScore < -0.2f)
                return EmotionType.Nervous; // Bot is slightly behind

            // Sometimes show random emotions
            if (Random.value < 0.1f)
            {
                var emotions = new[]
                {
                    EmotionType.Happy, EmotionType.Surprised,
                    EmotionType.Confused, EmotionType.Confident
                };
                return emotions[Random.Range(0, emotions.Length)];
            }

            // Default
            return EmotionType.None;
        }

        /// <summary>
        /// Generates a strategic move for the AI
        /// </summary>
        private PlayerMove GenerateMove()
        {
            var move = new PlayerMove();

            // In a full implementation, this would use the game state to make intelligent decisions

            // Bot plays 1-3 cards
            int cardsToPlay = Random.Range(1, 4);

            for (int i = 0; i < cardsToPlay; i++)
            {
                // Try to place units strategically - favor the middle row
                int row = GetWeightedRandomRow();
                int line = Random.Range(0, 3);

                var cardMove = new CardMove(
                    new CardLevel(
                        (CardId)Random.Range(0, 3), // Random card ID (0-2)
                        1),                        // Level 1
                    line,
                    row,
                    Index
                );

                move.AddCard(cardMove);
            }

            // Randomly decide whether to burn a card for mana (30% chance)
            if (Random.value < 0.3f)
            {
                move.BurnCardForMana(new CardLevel(
                    (CardId)Random.Range(0, 3),
                    1
                ));
            }

            return move;
        }

        /// <summary>
        /// Returns a weighted random row, preferring the middle row (1)
        /// </summary>
        private int GetWeightedRandomRow()
        {
            // 50% chance of middle row, 25% chance of top or bottom
            float value = Random.value;

            if (value < 0.25f)
                return 0;
            else if (value < 0.75f)
                return 1;
            else
                return 2;
        }
    }
}