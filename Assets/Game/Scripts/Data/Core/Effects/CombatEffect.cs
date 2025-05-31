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
            // First, process units with FirstHit skill for both players
            ProcessFirstHitUnits(gameState, 0); // Player 1 first hit units
            ProcessFirstHitUnits(gameState, 1); // Player 2 first hit units
            
            // Then process regular units for both players
            ProcessRegularUnits(gameState, 0); // Player 1 regular units
            ProcessRegularUnits(gameState, 1); // Player 2 regular units
            
            return true;
        }
        
        /// <summary>
        /// Processes all units with FirstHit for a player
        /// </summary>
        private void ProcessFirstHitUnits(GameState gameState, int playerIndex)
        {
            var playerState = gameState.GetState(playerIndex);
            
            // Iterate through all board positions
            for (int row = 0; row < 3; row++)
            {
                for (int line = 0; line < 3; line++)
                {
                    var place = playerState.Board.GetPlace(line, row);
                    
                    // Skip empty places or units without FirstHit
                    if (place.IsEmpty || !place.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                        continue;
                    
                    // Handle attack for this unit
                    AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    
                    // Handle double damage if applicable
                    if (place.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                    {
                        AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes all regular units (without FirstHit) for a player
        /// </summary>
        private void ProcessRegularUnits(GameState gameState, int playerIndex)
        {
            var playerState = gameState.GetState(playerIndex);
            
            // Iterate through all board positions
            for (int row = 0; row < 3; row++)
            {
                for (int line = 0; line < 3; line++)
                {
                    var place = playerState.Board.GetPlace(line, row);
                    
                    // Skip empty places or units with FirstHit (already processed)
                    if (place.IsEmpty || place.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                        continue;
                    
                    // Handle attack for this unit
                    AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    
                    // Handle double damage if applicable
                    if (place.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                    {
                        AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handles an attack from a unit, targeting enemies in the same row from front to back
        /// </summary>
        private void AttackInLine(GameState gameState, GameUnitData attacker, int attackerPlayerIndex, int attackerLine, int attackerRow)
        {
            int opponentIndex = 1 - attackerPlayerIndex; // 0 -> 1, 1 -> 0
            var opponentState = gameState.GetState(opponentIndex);
            
            Debug.Log($"Unit at position (line:{attackerLine}, row:{attackerRow}) from player {attackerPlayerIndex} is attacking in row {attackerRow}");
            
            // Try to find a target in the same row (attackerRow)
            // We look at all columns (lines) for the opponent in the same row
            for (int targetRow = 0; targetRow < 3; targetRow++)
            {
                var targetPlace = opponentState.Board.GetPlace(attackerLine, targetRow);
                
                if (!targetPlace.IsEmpty)
                {
                    // Target found, deal damage
                    Debug.Log($"Target found at position (line:{attackerLine}, row:{targetRow}) for player {opponentIndex}");
                    DealDamage(attacker, targetPlace, opponentState.Hero);
                    
                    // Found a target, so stop searching
                    return;
                }
            }
            
            // If no target found in the same row, damage the hero
            Debug.Log($"No targets in line {attackerLine}, attacking hero of player {opponentIndex}");
            DealDamage(attacker, null, opponentState.Hero);
        }
        
        private void DealDamage(GameUnitData attacker, GameBoardPlace targetPlace, HeroState targetHero)
        {
            int damage = attacker.UnitState.Damage;
            
            if (targetPlace == null || targetPlace.IsEmpty)
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
