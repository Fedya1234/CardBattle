using System.Collections.Generic;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that handles combat between units
    /// </summary>
    public class CombatEffect : BaseCardEffect
    {
        public CombatEffect() : base(EffectType.Combat)
        {
        }

        public override bool ApplyEffect(
            GameState gameState,
            int sourcePlayerIndex,
            int sourceLine,
            int sourceRow)
        {
            // Combat happens across all rows and lines
            for (int line = 0; line < 3; line++)
            {
                for (int row = 0; row < 3; row++)
                {
                    ProcessCombat(gameState, line, row);
                }
            }
            
            return true;
        }
        
        private void ProcessCombat(GameState gameState, int line, int row)
        {
            var player1 = gameState.GetState(0);
            var player2 = gameState.GetState(1);
            
            var place1 = player1.Board.GetPlace(line, row);
            var place2 = player2.Board.GetPlace(line, row);
            
            if (place1.IsEmpty && place2.IsEmpty)
            {
                // No units to fight
                return;
            }
            
            // Process units with FirstHit first
            if (!place1.IsEmpty && place1.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
            {
                // Player 1 unit attacks first
                DealDamage(place1.Unit, place2, player2.Hero);
            }
            
            if (!place2.IsEmpty && place2.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
            {
                // Player 2 unit attacks first
                DealDamage(place2.Unit, place1, player1.Hero);
            }
            
            // Regular attack from player 1
            if (!place1.IsEmpty && !place1.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
            {
                DealDamage(place1.Unit, place2, player2.Hero);
                
                // Handle double damage
                if (!place1.IsEmpty && place1.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(place1.Unit, place2, player2.Hero);
                }
            }
            
            // Regular attack from player 2
            if (!place2.IsEmpty && !place2.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
            {
                DealDamage(place2.Unit, place1, player1.Hero);
                
                // Handle double damage
                if (!place2.IsEmpty && place2.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(place2.Unit, place1, player1.Hero);
                }
            }
        }
        
        private void DealDamage(GameUnitData attacker, GameBoardPlace targetPlace, HeroState targetHero)
        {
            int damage = attacker.UnitState.Damage;
            
            if (targetPlace.IsEmpty)
            {
                // No unit, damage goes to hero
                targetHero.ChangeHealth(-damage);
                
                Debug.Log($"Unit attacks hero for {damage} damage");
            }
            else
            {
                // Attack unit
                var targetUnit = targetPlace.Unit;
                
                // Apply damage
                targetUnit.UnitState.AddHealth(-damage);
                
                Debug.Log($"Unit attacks unit for {damage} damage");
                
                // Check for vampirism
                if (attacker.UnitState.Skills.Contains(SkillId.Vampire))
                {
                    // Heal the attacker for the damage dealt (capped by the damage actually taken)
                    int healAmount = Mathf.Min(damage, damage);
                    attacker.UnitState.AddHealth(healAmount);
                    Debug.Log($"Unit heals for {healAmount} with vampirism");
                }
                
                // Check if target died
                if (targetUnit.UnitState.Health <= 0)
                {
                    targetPlace.KillUnit(targetUnit);
                    Debug.Log("Unit died from combat damage");
                }
            }
        }
    }
}