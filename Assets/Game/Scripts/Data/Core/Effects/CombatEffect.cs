using System.Collections.Generic;
using System.Linq;
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
            
            // Then process regular units "simultaneously"
            ProcessRegularUnitsConcurrently(gameState);
            
            return true;
        }
        
        /// <summary>
        /// Processes all units with FirstHit for a player (these have priority and attack first)
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
        /// Processes all regular units concurrently to simulate simultaneous attacks
        /// </summary>
        private void ProcessRegularUnitsConcurrently(GameState gameState)
        {
            // First collect all pending damage that will be applied simultaneously
            var pendingDamage = new List<PendingDamage>();
            
            // Collect damage from player 1 units
            CollectPendingDamage(gameState, 0, pendingDamage);
            
            // Collect damage from player 2 units
            CollectPendingDamage(gameState, 1, pendingDamage);
            
            // Apply all damage at once after collection
            ApplyAllPendingDamage(pendingDamage);
            
            // Apply secondary effects (like vampirism) and handle deaths
            ApplySecondaryEffects(pendingDamage);
        }
        
        /// <summary>
        /// Collects pending damage from all regular units for a player
        /// </summary>
        private void CollectPendingDamage(GameState gameState, int playerIndex, List<PendingDamage> pendingDamage)
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
                    
                    // Calculate potential damage from this unit
                    CalculateDamage(gameState, place.Unit, playerIndex, line, row, pendingDamage);
                    
                    // Handle double damage if applicable
                    if (place.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                    {
                        CalculateDamage(gameState, place.Unit, playerIndex, line, row, pendingDamage);
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculates the damage that will be dealt by a unit
        /// </summary>
        private void CalculateDamage(
            GameState gameState, 
            GameUnitData attacker, 
            int attackerPlayerIndex, 
            int attackerLine, 
            int attackerRow, 
            List<PendingDamage> pendingDamage)
        {
            int opponentIndex = 1 - attackerPlayerIndex; // 0 -> 1, 1 -> 0
            var opponentState = gameState.GetState(opponentIndex);
            
            Debug.Log($"Unit at position (line:{attackerLine}, row:{attackerRow}) from player {attackerPlayerIndex} is calculating damage in line {attackerLine}");
            
            // Try to find a target in the same line (vertical column)
            for (int targetRow = 0; targetRow < 3; targetRow++)
            {
                var targetPlace = opponentState.Board.GetPlace(attackerLine, targetRow);
                
                if (!targetPlace.IsEmpty)
                {
                    // Target found, prepare damage
                    var pendingDamageToThisTarget = pendingDamage
                        .Where(d => d.TargetPlace == targetPlace)
                        .Sum(d => d.Amount);
                    
                    if (pendingDamageToThisTarget >= targetPlace.Unit.UnitState.Health)
                    {
                        // If this unit's damage would kill the target, skip it
                        Debug.Log($"Skipping attack on target at (line:{attackerLine}, row:{targetRow}) - already lethal damage pending");
                        continue;
                    }
                        
                    Debug.Log($"Target found at position (line:{attackerLine}, row:{targetRow}) for player {opponentIndex}");
                    
                    pendingDamage.Add(new PendingDamage {
                        Attacker = attacker,
                        Target = targetPlace.Unit,
                        TargetPlace = targetPlace,
                        TargetHero = null,
                        Amount = attacker.UnitState.Damage
                    });
                    
                    // Found a target, so stop searching
                    return;
                }
            }
            
            // If no target found in the same line, damage goes to hero
            Debug.Log($"No targets in line {attackerLine}, preparing attack on hero of player {opponentIndex}");
            
            pendingDamage.Add(new PendingDamage {
                Attacker = attacker,
                Target = null,
                TargetPlace = null,
                TargetHero = opponentState.Hero,
                Amount = attacker.UnitState.Damage
            });
        }
        
        /// <summary>
        /// Apply all pending damage simultaneously
        /// </summary>
        private void ApplyAllPendingDamage(List<PendingDamage> pendingDamage)
        {
            foreach (var damage in pendingDamage)
            {
                if (damage.TargetHero != null)
                {
                    // Apply damage to hero
                    damage.TargetHero.ChangeHealth(-damage.Amount);
                    Debug.Log($"Unit deals {damage.Amount} damage to hero");
                }
                else if (damage.Target != null)
                {
                    // Apply damage to unit
                    damage.Target.UnitState.AddHealth(-damage.Amount);
                    Debug.Log($"Unit deals {damage.Amount} damage to unit");
                }
            }
        }
        
        /// <summary>
        /// Apply secondary effects and handle deaths after damage has been applied
        /// </summary>
        private void ApplySecondaryEffects(List<PendingDamage> pendingDamage)
        {
            foreach (var damage in pendingDamage)
            {
                // Skip if target was hero
                if (damage.Target == null)
                    continue;
                
                // Check for vampirism
                if (damage.Attacker.UnitState.Skills.Contains(SkillId.Vampire))
                {
                    // Heal the attacker for the damage dealt
                    int healAmount = Mathf.Min(damage.Amount, damage.Amount); // Could be modified if we track actual damage taken
                    damage.Attacker.UnitState.AddHealth(healAmount);
                    Debug.Log($"Unit heals for {healAmount} with vampirism");
                }
                
                // Check if target died
                if (damage.Target.UnitState.Health <= 0 && damage.TargetPlace != null)
                {
                    damage.TargetPlace.KillUnit(damage.Target);
                    Debug.Log("Unit died from combat damage");
                }
            }
        }
        
        /// <summary>
        /// Handles an attack from a unit with FirstHit, targeting enemies in the same line
        /// </summary>
        private void AttackInLine(GameState gameState, GameUnitData attacker, int attackerPlayerIndex, int attackerLine, int attackerRow)
        {
            int opponentIndex = 1 - attackerPlayerIndex; // 0 -> 1, 1 -> 0
            var opponentState = gameState.GetState(opponentIndex);
            
            Debug.Log($"FirstHit unit at position (line:{attackerLine}, row:{attackerRow}) from player {attackerPlayerIndex} is attacking in line {attackerLine}");
            
            // Try to find a target in the same line (vertical column)
            for (int targetRow = 0; targetRow < 3; targetRow++)
            {
                var targetPlace = opponentState.Board.GetPlace(attackerLine, targetRow);
                
                if (!targetPlace.IsEmpty)
                {
                    // Target found, deal damage immediately (FirstHit privilege)
                    Debug.Log($"Target found at position (line:{attackerLine}, row:{targetRow}) for player {opponentIndex}");
                    DealDamage(attacker, targetPlace, opponentState.Hero);
                    
                    // Found a target, so stop searching
                    return;
                }
            }
            
            // If no target found in the same line, damage the hero
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
        
        /// <summary>
        /// Class to store pending damage information
        /// </summary>
        private class PendingDamage
        {
            public GameUnitData Attacker;
            public GameUnitData Target;
            public GameBoardPlace TargetPlace;
            public HeroState TargetHero;
            public int Amount;
        }
    }
}
