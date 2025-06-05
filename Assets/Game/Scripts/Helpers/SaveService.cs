using UnityEngine;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using Game.Scripts.Data.Static;
using Game.Scripts.Save;
using System.Collections.Generic;

namespace Game.Scripts.Helpers
{
    /// <summary>
    /// Static service for managing save data across the game
    /// </summary>
    public static class SaveService
    {
        // The main save data instance
        private static SaveData _saveData;
        
        // Cache for player save data to avoid repeated serialization/deserialization
        private static PlayerSave _playerSaveCache;
        
        // Flag to track if we've attempted to load the data
        private static bool _isInitialized;
        
        /// <summary>
        /// Initialize the service and load save data
        /// </summary>
        private static void Initialize()
        {
            if (_isInitialized)
                return;
                
            // Load save data from Resources
            _saveData = Resources.Load<SaveData>("SaveData");
            
            if (_saveData == null)
            {
                Debug.LogWarning("Failed to load SaveData from Resources! Creating a new instance.");
                _saveData = ScriptableObject.CreateInstance<SaveData>();
                
                // Set default values if needed
                _saveData.SetToDefault();
            }
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Gets the player save data
        /// </summary>
        public static PlayerSave GetPlayerSave()
        {
            Initialize();
            
            if (_playerSaveCache == null)
            {
                _playerSaveCache = _saveData.PlayerSave;
                
                // If still null, create a default save
                if (_playerSaveCache == null)
                {
                    Debug.LogWarning("PlayerSave not initialized. Creating default save data.");
                    _playerSaveCache = new PlayerSave
                    {
                        HeroId = HeroId.Default,
                        Level = 1,
                        Cards = new List<CardSave>()
                    };
                    _saveData.PlayerSave = _playerSaveCache;
                }
            }
            
            return _playerSaveCache;
        }
        
        /// <summary>
        /// Updates the player save data
        /// </summary>
        public static void UpdatePlayerSave(PlayerSave playerSave)
        {
            Initialize();
            
            _saveData.PlayerSave = playerSave;
            _playerSaveCache = playerSave;
            
            // Save changes immediately
            SaveChanges();
        }
        
        /// <summary>
        /// Gets hero data based on player's current hero and level
        /// </summary>
        public static HeroData GetHeroData()
        {
            var playerSave = GetPlayerSave();
            return StaticDataService.GetHeroData(playerSave.HeroId);
        }
        
        /// <summary>
        /// Gets the player's current hero health
        /// </summary>
        public static int GetPlayerHealth()
        {
            var playerSave = GetPlayerSave();
            return StaticDataService.GetHeroHealth(playerSave.HeroId);
        }
        
        /// <summary>
        /// Updates the player hero
        /// </summary>
        public static void UpdatePlayerHero(HeroId heroId)
        {
            var playerSave = GetPlayerSave();
            playerSave.HeroId = heroId;
            SaveChanges();
        }
        
        /// <summary>
        /// Updates the player level
        /// </summary>
        public static void UpdatePlayerLevel(int level)
        {
            var playerSave = GetPlayerSave();
            playerSave.Level = level;
            SaveChanges();
        }
        
        /// <summary>
        /// Updates the player cards collection
        /// </summary>
        public static void UpdatePlayerCards(List<CardSave> cards)
        {
            var playerSave = GetPlayerSave();
            playerSave.Cards = cards;
            SaveChanges();
        }
        
        /// <summary>
        /// Adds a card to the player's collection
        /// </summary>
        public static void AddCard(CardSave card)
        {
            var playerSave = GetPlayerSave();
            
            if (playerSave.Cards == null)
                playerSave.Cards = new List<CardSave>();
            
            // Check if card already exists to update quantity
            bool cardFound = false;
            foreach (var existingCard in playerSave.Cards)
            {
                if (existingCard.Id == card.Id && existingCard.Level == card.Level)
                {
                    existingCard.Count += card.Count;
                    cardFound = true;
                    break;
                }
            }
            
            // If card wasn't found, add it
            if (!cardFound)
                playerSave.Cards.Add(card);
            
            SaveChanges();
        }
        
        /// <summary>
        /// Removes a card from the player's collection
        /// </summary>
        public static bool RemoveCard(CardSave card)
        {
            var playerSave = GetPlayerSave();
            
            if (playerSave.Cards == null)
                return false;
            
            for (int i = 0; i < playerSave.Cards.Count; i++)
            {
                var existingCard = playerSave.Cards[i];
                if (existingCard.Id == card.Id && existingCard.Level == card.Level)
                {
                    if (existingCard.Count > card.Count)
                    {
                        existingCard.Count -= card.Count;
                    }
                    else
                    {
                        playerSave.Cards.RemoveAt(i);
                    }
                    SaveChanges();
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Saves all changes to disk
        /// </summary>
        public static void SaveChanges()
        {
            Initialize();
            _saveData.Save();
        }
        
        /// <summary>
        /// Resets save data to default values
        /// </summary>
        public static void ResetSaveData()
        {
            Initialize();
            _saveData.SetToDefault();
            _playerSaveCache = _saveData.PlayerSave;
            SaveChanges();
        }
    }
}