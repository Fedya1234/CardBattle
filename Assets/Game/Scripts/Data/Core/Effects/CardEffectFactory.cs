using System.Collections.Generic;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Factory for creating card effects based on card properties
    /// </summary>
    public class CardEffectFactory
    {
        private readonly Dictionary<MagicId, ICardEffect> _magicEffectCache = new Dictionary<MagicId, ICardEffect>();
        private readonly Dictionary<SkillId, ICardEffect> _skillEffectCache = new Dictionary<SkillId, ICardEffect>();
        
        /// <summary>
        /// Create unit placement effect for a unit card
        /// </summary>
        public ICardEffect CreateUnitPlacementEffect(CardId cardId, UnitData unitData)
        {
            var cardStaticData = StaticDataService.GetCardData(cardId);
            return new PlaceUnitEffect(unitData, cardStaticData.ManaCost);
        }

        /// <summary>
        /// Create magic effect for a magic card
        /// </summary>
        public ICardEffect CreateMagicEffect(CardId cardId)
        {
            //var magicCardVisual = VisualService.GetMagicCardVisual(cardId);
            
            // Try to determine magic ID based on card ID
            MagicId magicId = DetermineMagicId(cardId);
            
            // Use cached effect if available
            if (_magicEffectCache.TryGetValue(magicId, out var cachedEffect))
            {
                return cachedEffect;
            }
            
            // Create new effect based on magic ID
            ICardEffect effect;
            switch (magicId)
            {
                case MagicId.Magic1:
                    // Get mana cost for damage calculation
                    int manaCost = StaticDataService.GetCardData(cardId).ManaCost;
                    effect = new DamageEffect(manaCost * 2); // Example: Deal damage based on mana cost
                    break;
                default:
                    Debug.LogWarning($"Unknown magic ID for card: {cardId}");
                    effect = new NoEffect();
                    break;
            }
            
            _magicEffectCache[magicId] = effect;
            return effect;
        }
        
        /// <summary>
        /// Create skill effect for a unit skill
        /// </summary>
        public ICardEffect CreateSkillEffect(SkillId skillId)
        {
            // Use cached effect if available
            if (_skillEffectCache.TryGetValue(skillId, out var cachedEffect))
            {
                return cachedEffect;
            }
            
            // Create new effect based on skill ID
            ICardEffect effect;
            switch (skillId)
            {
                case SkillId.DoubleDamage:
                    effect = new DoubleDamageEffect();
                    break;
                case SkillId.Vampire:
                    effect = new VampirismEffect();
                    break;
                case SkillId.FirstHit:
                    effect = new FirstStrikeEffect();
                    break;
                case SkillId.AntiMagic:
                    effect = new AntiMagicEffect();
                    break;
                case SkillId.Armor:
                    effect = new ArmorEffect();
                    break;
                case SkillId.EndTurnHealth_1:
                    effect = new EndTurnHealEffect(1);
                    break;
                default:
                    Debug.LogWarning($"Unknown skill ID: {skillId}");
                    effect = new NoEffect();
                    break;
            }
            
            _skillEffectCache[skillId] = effect;
            return effect;
        }
        
        /// <summary>
        /// Create hero ability effect
        /// </summary>
        public ICardEffect CreateHeroAbilityEffect(MagicId abilityId)
        {
            // For now, hero abilities use the same effects as magic cards
            if (_magicEffectCache.TryGetValue(abilityId, out var cachedEffect))
            {
                return cachedEffect;
            }
            
            ICardEffect effect;
            switch (abilityId)
            {
                case MagicId.Magic1:
                    effect = new DamageEffect(1); // Example: Deal 1 damage
                    break;
                default:
                    Debug.LogWarning($"Unknown hero ability ID: {abilityId}");
                    effect = new NoEffect();
                    break;
            }
            
            _magicEffectCache[abilityId] = effect;
            return effect;
        }
        
        /// <summary>
        /// Determines the MagicId based on CardId
        /// </summary>
        private MagicId DetermineMagicId(CardId cardId)
        {
            // This is a simple mapping function that could be expanded
            // or replaced with data from a configuration file
            switch (cardId)
            {
                case CardId.Card_1:
                    return MagicId.Magic1;
                default:
                    return MagicId.Default;
            }
        }
    }
}
