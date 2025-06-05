using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Core
{
    public class GameInitializer
    {
        // Direct reference to CardGenerator class to ensure it's loaded
        private static readonly System.Type _cardGeneratorType = typeof(CardGenerator);
        
        private OpponentFinder _opponentFinder = new OpponentFinder();
        private GameSession _gameSession;
        
        /// <summary>
        /// Event raised when the game session is created and initialized
        /// </summary>
        public delegate void GameSessionCreatedHandler(GameSession session);
        public event GameSessionCreatedHandler OnGameSessionCreated;
        
        public async UniTask StartGame()
        {
            Debug.Log("Starting game...");
            
            // Get player save
            var save = GetSave();
            
            // Find an opponent
            var opponent = await _opponentFinder.FindOpponent(save);
            
            // Create and start the game session
            _gameSession = new GameSession(new[] { save, opponent });
            
            // Notify listeners that the game session is ready
            OnGameSessionCreated?.Invoke(_gameSession);
            
            // Start the game
            await _gameSession.StartGame();
        }

        private PlayerSave GetSave()
        {
            // Get player save from SaveService
            var playerSave = SaveService.GetPlayerSave();
            
            // If this is a new player without any cards, generate a starter deck
            if (playerSave.Cards == null || playerSave.Cards.Count == 0)
            {
                playerSave.Cards = GenerateStarterDeck();
                SaveService.UpdatePlayerCards(playerSave.Cards);
            }
            
            // Ensure the player has a valid hero
            if (playerSave.HeroId == HeroId.Default)
            {
                playerSave.HeroId = HeroId.Warrior; // Default to Warrior
                SaveService.UpdatePlayerHero(HeroId.Warrior);
            }
            
            return playerSave;
        }
        
        /// <summary>
        /// Gets the current game session (if it exists)
        /// </summary>
        public GameSession GetGameSession()
        {
            return _gameSession;
        }

        /// <summary>
        /// Utility method to generate a starter deck
        /// </summary>
        private List<CardSave> GenerateStarterDeck()
        {
            try
            {
                return CardGenerator.GenerateStarterDeck();
            }
            catch
            {
                return GenerateStarterDeckFallback();
            }
        }

        /// <summary>
        /// Fallback method to generate a starter deck if CardGenerator can't be found
        /// </summary>
        private List<CardSave> GenerateStarterDeckFallback()
        {
            Debug.LogWarning("Using fallback starter deck generation because CardGenerator couldn't be found");
            
            var deck = new List<CardSave>
            {
                // Basic unit cards
                new CardSave(CardId.Card_0, 1, 8),  // 8 basic units
                
                // Basic damage spell
                new CardSave(CardId.Card_1, 1, 4),  // 4 damage spells
                
                // Basic buff spell
                new CardSave(CardId.Card_2, 1, 3)   // 3 buff spells
            };
            
            return deck;
        }
    }
}