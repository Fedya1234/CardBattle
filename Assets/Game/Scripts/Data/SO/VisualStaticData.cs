using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Visual;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Data.SO
{
    [CreateAssetMenu(menuName = "SO/VisualStaticData", fileName = "VisualStaticData")]
    public class VisualStaticData : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<CardId, CardVisual> _cards;
        
        
        public CardVisual GetCardVisual(CardId cardId)
        {
            if (_cards.ContainsKey(cardId) == false)
            {
                Debug.LogError($"CardVisual not found for {cardId}");
                return new CardVisual();
            }
            return _cards[cardId];
        }
    }
}