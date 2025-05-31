using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Visual;
using UnityEngine;

namespace Game.Scripts.Helpers
{
    /// <summary>
    /// Service class for managing visual data across the game
    /// Static implementation for direct access to visual data methods
    /// </summary>
    public static class VisualService
    {
        // Reference to visual static data (cached from Resources)
        private static VisualStaticData _visualData;
        
        // Internal caches to avoid repeated lookups
        private static readonly Dictionary<CardId, CardVisual> _cardVisualCache = new Dictionary<CardId, CardVisual>();
        private static readonly Dictionary<CardId, UnitCardVisual> _unitCardVisualCache = new Dictionary<CardId, UnitCardVisual>();
        private static readonly Dictionary<CardId, MagicCardVisual> _magicCardVisualCache = new Dictionary<CardId, MagicCardVisual>();
        
        // Static constructor
        static VisualService()
        {
            Initialize();
        }
        
        /// <summary>
        /// Initialize the service and load necessary data
        /// </summary>
        private static void Initialize()
        {
            // Load visual data from Resources
            _visualData = Resources.Load<VisualStaticData>("VisualStaticData");
            
            if (_visualData == null)
            {
                Debug.LogError("Failed to load VisualStaticData from Resources!");
            }
        }
        
        public static CardTypeId GetCardTypeId(CardId cardId)
        {
            // Check if we have visual data cached
            if (_visualData == null)
            {
                Debug.LogError("VisualStaticData is not loaded!");
                return default;
            }
            
            // Use the private method from VisualStaticData through appropriate accessor
            return _visualData.GetCardTypeId(cardId);
        }
        /// <summary>
        /// Gets general card visual data for the specified card ID
        /// </summary>
        public static CardVisual GetCardVisual(CardId cardId)
        {
            // Check if we have visual data cached
            if (!_cardVisualCache.TryGetValue(cardId, out var cardVisual))
            {
                // Try to get from visual data
                if (_visualData != null)
                {
                    // Use the private method from VisualStaticData through appropriate accessor
                    var unitVisual = _visualData.GetUnitCardVisual(cardId);
                    if (unitVisual != null)
                    {
                        cardVisual = unitVisual;
                    }
                    else
                    {
                        var magicVisual = _visualData.GetMagicCardVisual(cardId);
                        if (magicVisual != null)
                        {
                            cardVisual = magicVisual;
                        }
                    }
                }
                
                if (cardVisual == null)
                {
                    Debug.LogError($"Card visual data not found for CardId: {cardId}");
                    return null;
                }
                
                // Cache it
                _cardVisualCache[cardId] = cardVisual;
            }
            
            return cardVisual;
        }
        
        /// <summary>
        /// Gets unit card visual data for the specified card ID
        /// </summary>
        public static UnitCardVisual GetUnitCardVisual(CardId cardId)
        {
            // Check if we have unit visual data cached
            if (!_unitCardVisualCache.TryGetValue(cardId, out var unitCardVisual))
            {
                // Try to get from visual data
                if (_visualData != null)
                {
                    unitCardVisual = _visualData.GetUnitCardVisual(cardId);
                }
                
                if (unitCardVisual == null)
                {
                    Debug.LogError($"Unit card visual data not found for CardId: {cardId}");
                    return null;
                }
                
                // Cache it
                _unitCardVisualCache[cardId] = unitCardVisual;
            }
            
            return unitCardVisual;
        }
        
        /// <summary>
        /// Gets magic card visual data for the specified card ID
        /// </summary>
        public static MagicCardVisual GetMagicCardVisual(CardId cardId)
        {
            // Check if we have magic visual data cached
            if (!_magicCardVisualCache.TryGetValue(cardId, out var magicCardVisual))
            {
                // Try to get from visual data
                if (_visualData != null)
                {
                    magicCardVisual = _visualData.GetMagicCardVisual(cardId);
                }
                
                if (magicCardVisual == null)
                {
                    Debug.LogError($"Magic card visual data not found for CardId: {cardId}");
                    return null;
                }
                
                // Cache it
                _magicCardVisualCache[cardId] = magicCardVisual;
            }
            
            return magicCardVisual;
        }
    }
}