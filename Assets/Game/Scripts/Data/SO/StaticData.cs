using System.Collections.Generic;
using Game.Scripts.Data.Core;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Data.SO
{
    [CreateAssetMenu(menuName = "SO/StaticData", fileName = "StaticData")]
    public class StaticData : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<CardId, CardLevelData> _cards;
        
        public CardLevelData GetCardLevelData(CardId cardId)
        {
            if (_cards.ContainsKey(cardId) == false)
            {
                Debug.LogError($"CardLevelData not found for {cardId}");
                return new CardLevelData();
            }
            return _cards[cardId];
        }

        public CardData GetCardData(CardId cardId, int level)
        {
            var cardLevelData = GetCardLevelData(cardId);
            var cardData = cardLevelData.DataByLevel;
            if (cardData.Length == 0 || level < 0)
            {
                Debug.LogError($"CardData not found for {cardId} and level {level}");
                return new CardData();
            }
            
            if (level >= cardData.Length)
            {
                Debug.LogWarning($"CardData not found for {cardId} and level {level}");
            }
            
            return cardData[Mathf.Min(level, cardData.Length - 1)];
        }
    }
}