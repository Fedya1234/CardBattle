using Game.Scripts.Data.Core.State;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect for dealing damage to units or heroes
    /// </summary>
    public class DamageEffect : BaseCardEffect
    {
        private readonly int _damage;
        private readonly bool _targetHero;
        
        /// <summary>
        /// Create a damage effect
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        /// <param name="targetHero">If true, targets the hero directly; otherwise targets a unit</param>
        public DamageEffect(int damage, bool targetHero = false) : base(EffectType.NegativeEffect)
        {
            _damage = damage;
            _targetHero = targetHero;
        }

        public override bool ApplyEffect(
            GameState gameState, 
            int sourcePlayerIndex, 
            int sourceLine, 
            int sourceRow)
        {
            // Get target player (opponent)
            int targetPlayerIndex = GetTargetPlayerIndex(sourcePlayerIndex);
            var targetPlayerState = gameState.GetState(targetPlayerIndex);
            
            if (_targetHero)
            {
                // Direct damage to hero
                targetPlayerState.Hero.ChangeHealth(-_damage);
                return true;
            }
            else
            {
                // Damage to unit
                int targetRow = GetOpponentMatchingRow(sourceRow);
                var boardPlace = targetPlayerState.Board.GetPlace(sourceLine, targetRow);
                
                if (boardPlace.IsEmpty)
                {
                    // No unit to damage, so damage goes to hero instead
                    targetPlayerState.Hero.ChangeHealth(-_damage);
                    return true;
                }
                
                // Check if unit has AntiMagic skill
                if (boardPlace.Unit.UnitState.Skills.Contains(Data.Enums.SkillId.AntiMagic))
                {
                    Debug.Log("Unit has Anti-Magic, spell has no effect");
                    return false;
                }
                
                // Check if unit has Armor skill
                if (boardPlace.Unit.UnitState.Skills.Contains(Data.Enums.SkillId.Armor))
                {
                    // Armor reduces damage by 1 (min 0)
                    int reducedDamage = Mathf.Max(0, _damage - 1);
                    boardPlace.Unit.UnitState.AddHealth(-reducedDamage);
                    Debug.Log($"Unit has Armor, damage reduced to {reducedDamage}");
                }
                else
                {
                    // Normal damage
                    boardPlace.Unit.UnitState.AddHealth(-_damage);
                }
                
                // Check if unit died
                if (boardPlace.Unit.UnitState.Health <= 0)
                {
                    boardPlace.KillUnit(boardPlace.Unit);
                    Debug.Log("Unit died from damage");
                }
                
                return true;
            }
        }
    }
}