using System.Collections.Generic;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using UnityEngine;

namespace Game.Scripts.Helpers
{
    /// <summary>
    /// Service class for managing all static game data including cards, heroes, and units
    /// Static implementation for direct access to data methods
    /// </summary>
    public static class StaticDataService
    {
        // Reference to static data (cached from Resources)
        private static StaticData _staticData;
        
        // Internal caches to avoid repeated lookups
        private static readonly Dictionary<CardId, CardStaticData> _cardDataCache = new Dictionary<CardId, CardStaticData>();
        private static readonly Dictionary<UnitId, UnitStaticData> _unitDataCache = new Dictionary<UnitId, UnitStaticData>();
        private static readonly Dictionary<HeroId, HeroData> _heroDataCache = new Dictionary<HeroId, HeroData>();
        private static readonly Dictionary<HeroMagicId, HeroMagicData> _heroMagicDataCache = new ();
        
        // Flag to track initialization status
        private static bool _isInitialized;
        
        private static void Initialize()
        {
            if (_isInitialized)
                return;
                
            // Load static data from Resources
            _staticData = Resources.Load<StaticData>("StaticData");
            
            if (_staticData == null)
            {
                Debug.LogError("Failed to load StaticData from Resources! Using default values.");
            }
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Gets direct access to the raw StaticData
        /// Use with caution as this bypasses caching
        /// </summary>
        public static StaticData GetStaticData()
        {
            Initialize();
            return _staticData;
        }
        
        #region Card Data Methods
        
        /// <summary>
        /// Gets static data for a specific card
        /// </summary>
        public static CardStaticData GetCardData(CardId cardId)
        {
            Initialize();
            // Check if we have base card data cached
            if (!_cardDataCache.TryGetValue(cardId, out var cardData))
            {
                // Try to get from static data
                if (_staticData != null)
                {
                    cardData = _staticData.GetCardStaticData(cardId);
                }
                
                if (cardData == null)
                {
                    Debug.LogError($"Card data not found for CardId: {cardId}");
                    return null;
                }
                
                // Cache it
                _cardDataCache[cardId] = cardData;
            }
            
            return cardData;
        }
        
        /// <summary>
        /// Gets mana cost for a specific card
        /// </summary>
        public static int GetCardManaCost(CardId cardId)
        {
            Initialize();
            var cardData = GetCardData(cardId);
            return cardData?.ManaCost ?? 1; // Default to 1 if data not found
        }
        
        #endregion
        
        #region Unit Data Methods
        
        /// <summary>
        /// Gets static data for a specific unit
        /// </summary>
        public static UnitStaticData GetUnitData(UnitId unitId, int level)
        {
            Initialize();
            var unitCacheKey = unitId;
            
            // Check if we have unit data cached
            if (!_unitDataCache.TryGetValue(unitCacheKey, out var unitData))
            {
                // Try to get from static data
                if (_staticData != null)
                {
                    var unitBaseData = new UnitData { Id = unitId, Level = level };
                    unitData = _staticData.GetUnitStaticData(unitBaseData);
                }
                
                if (unitData == null)
                {
                    Debug.LogError($"Unit data not found for UnitId: {unitId}, Level: {level}");
                    return null;
                }
                
                // Cache it
                _unitDataCache[unitCacheKey] = unitData;
            }
            
            return unitData;
        }
        
        #endregion
        
        #region Hero Data Methods
        
        /// <summary>
        /// Gets hero data for a specific hero
        /// </summary>
        public static HeroData GetHeroData(HeroId heroId)
        {
            Initialize();
            // Check if we have hero data cached
            if (!_heroDataCache.TryGetValue(heroId, out var heroData))
            {
                // Try to get from static data
                if (_staticData != null)
                {
                    heroData = _staticData.GetHeroData(heroId);
                }
                
                if (heroData == null)
                {
                    Debug.LogError($"Hero data not found for HeroId: {heroId}");
                    return new HeroData(20); // Default values if not found
                }
                
                // Cache it
                _heroDataCache[heroId] = heroData;
            }
            
            return heroData;
        }
        
        /// <summary>
        /// Gets health for a specific hero
        /// </summary>
        public static int GetHeroHealth(HeroId heroId)
        {
            Initialize();
            var heroData = GetHeroData(heroId);
            return heroData.Health;
        }
        
        #endregion
        
        
        #region Hero Magic Data Methods
        public static HeroMagicData GetHeroMagicData(HeroMagicId magicId)
        {
            Initialize();
            // Check if we have hero magic data cached
            if (!_heroMagicDataCache.TryGetValue(magicId, out var heroMagicData))
            {
                // Try to get from static data
                if (_staticData != null)
                {
                    heroMagicData = _staticData.GetHeroMagicData(magicId);
                }
                
                if (heroMagicData == null)
                {
                    Debug.LogError($"Hero magic data not found for HeroMagicId: {magicId}");
                    return null;
                }
                
                // Cache it
                _heroMagicDataCache[magicId] = heroMagicData;
            }
            
            return heroMagicData;
        }
        #endregion
        
        /// <summary>
        /// Clears all cached data, forcing a reload on next access
        /// </summary>
        public static void ClearCache()
        {
            _cardDataCache.Clear();
            _unitDataCache.Clear();
            _heroDataCache.Clear();
        }
    }
}