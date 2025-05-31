using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Data.Visual
{
    [CreateAssetMenu(menuName = "SO/VisualStaticData", fileName = "VisualStaticData")]
    public class VisualStaticData : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<CardId, CardVisual> _cards;
        [SerializeField] private Dictionary<HeroId, HeroVisual> _heroes;
        [SerializeField] private Dictionary<HeroMagicId, HeroMagicVisual> _heroMagics;
        
        
        public HeroVisual GetHeroVisual(HeroId heroId)
        {
            if (_heroes.ContainsKey(heroId) == false)
            {
                Debug.LogError($"HeroVisual not found for {heroId}");
                return new HeroVisual();
            }
            return _heroes[heroId];
        }
        
        public HeroMagicVisual GetHeroMagicVisual(HeroMagicId heroMagicId)
        {
            if (_heroMagics.ContainsKey(heroMagicId) == false)
            {
                Debug.LogError($"HeroMagicVisual not found for {heroMagicId}");
                return new HeroMagicVisual();
            }
            return _heroMagics[heroMagicId];
        }
        
        public UnitCardVisual GetUnitCardVisual(CardId cardId)
        {
            var cardVisual = GetCardVisual(cardId);
            if (cardVisual == null)
            {
                Debug.LogError($"CardVisual not found for {cardId}");
                return new UnitCardVisual();
            }
            
            return cardVisual as UnitCardVisual;
        }
        
        public CardTypeId GetCardTypeId(CardId cardId)
        {
            var cardVisual = GetCardVisual(cardId);
            if (cardVisual == null)
            {
                Debug.LogError($"CardVisual not found for {cardId}");
                return default;
            }
            
            return cardVisual.Type;
        }

        public MagicCardVisual GetMagicCardVisual(CardId cardId)
        {
            var cardVisual = GetCardVisual(cardId);
            if (cardVisual == null)
            {
                Debug.LogError($"CardVisual not found for {cardId}");
                return new MagicCardVisual();
            }
            
            return cardVisual as MagicCardVisual;
        }

        private CardVisual GetCardVisual(CardId cardId)
        {
            if (_cards.ContainsKey(cardId) == false)
            {
                Debug.LogError($"CardVisual not found for {cardId}");
                return null;
            }
            return _cards[cardId];
        }
    }
}