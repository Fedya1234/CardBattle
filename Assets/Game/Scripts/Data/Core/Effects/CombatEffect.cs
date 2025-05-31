using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.UI;
using Game.Scripts.UI.Board;
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
            // Start async combat process
            _ = ApplyEffectAsync(gameState, sourcePlayerIndex, sourceLine, sourceRow);
            return true;
        }
        
        private async UniTask ApplyEffectAsync(
            GameState gameState,
            int sourcePlayerIndex,
            int sourceLine,
            int sourceRow)
        {
            // First, process units with FirstHit skill for both players
            await ProcessFirstHitUnitsAsync(gameState, 0); // Player 1 first hit units
            await ProcessFirstHitUnitsAsync(gameState, 1); // Player 2 first hit units
            
            // Then process regular units line by line with delays
            await ProcessRegularUnitsConcurrentlyAsync(gameState);
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

        private async UniTask ProcessFirstHitUnitsAsync(GameState gameState, int playerIndex)
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
                    
                    // Play attack animation
                    var boardUnit = GetBoardUnitForUnit(gameState, place.Unit, playerIndex);
                    if (boardUnit != null)
                    {
                        await boardUnit.PlayAttackAnimation();
                    }
                    
                    // Handle attack for this unit
                    AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    
                    // Handle double damage if applicable
                    if (place.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
                    {
                        AttackInLine(gameState, place.Unit, playerIndex, line, row);
                    }
                    
                    // Add delay between attacks
                    await UniTask.DelayFrame(5);
                }
            }
        }
        
        /// <summary>
        /// Processes all regular units concurrently to simulate simultaneous attacks at the same position
        /// </summary>
        private void ProcessRegularUnitsConcurrently(GameState gameState)
        {
            // Process each line
            for (int line = 0; line < 3; line++)
            {
                ProcessLineCombat(gameState, line);
            }
        }
        
        private async UniTask ProcessRegularUnitsConcurrentlyAsync(GameState gameState)
        {
            // Process each line with delays
            for (int line = 0; line < 3; line++)
            {
                await ProcessLineCombatAsync(gameState, line);
                await UniTask.DelayFrame(10); // Add delay between lines
            }
        }
        
        /// <summary>
        /// Processes combat for an entire line, pairing units from both players
        /// </summary>
        private void ProcessLineCombat(GameState gameState, int line)
        {
            var player1State = gameState.GetState(0);
            var player2State = gameState.GetState(1);
            
            // Get all active units in this line for both players
            var player1Units = new List<(GameUnitData unit, int position)>();
            var player2Units = new List<(GameUnitData unit, int position)>();
            
            for (int pos = 0; pos < 3; pos++)
            {
                var place1 = player1State.Board.GetPlace(line, pos);
                var place2 = player2State.Board.GetPlace(line, pos);
                
                if (!place1.IsEmpty && !place1.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                {
                    player1Units.Add((place1.Unit, pos));
                }
                
                if (!place2.IsEmpty && !place2.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                {
                    player2Units.Add((place2.Unit, pos));
                }
            }
            
            Debug.Log($"Line {line}: Player 1 has {player1Units.Count} units, Player 2 has {player2Units.Count} units");
            
            // Process units in pairs (first with first, second with second, etc.)
            int maxUnits = Mathf.Max(player1Units.Count, player2Units.Count);
            
            for (int i = 0; i < maxUnits; i++)
            {
                GameUnitData unit1 = i < player1Units.Count ? player1Units[i].unit : null;
                GameUnitData unit2 = i < player2Units.Count ? player2Units[i].unit : null;
                
                if (unit1 != null && unit2 != null)
                {
                    // Both units exist, simultaneous combat
                    ProcessPairedCombat(gameState, unit1, unit2, 0, 1, line);
                }
                else if (unit1 != null)
                {
                    // Only player 1 unit exists
                    ProcessSingleUnitAttack(gameState, unit1, 0, line);
                }
                else if (unit2 != null)
                {
                    // Only player 2 unit exists
                    ProcessSingleUnitAttack(gameState, unit2, 1, line);
                }
            }
        }

        private async UniTask ProcessLineCombatAsync(GameState gameState, int line)
        {
            var player1State = gameState.GetState(0);
            var player2State = gameState.GetState(1);
            
            // Get all active units in this line for both players
            var player1Units = new List<(GameUnitData unit, int position)>();
            var player2Units = new List<(GameUnitData unit, int position)>();
            
            for (int pos = 0; pos < 3; pos++)
            {
                var place1 = player1State.Board.GetPlace(line, pos);
                var place2 = player2State.Board.GetPlace(line, pos);
                
                if (!place1.IsEmpty && !place1.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                {
                    player1Units.Add((place1.Unit, pos));
                }
                
                if (!place2.IsEmpty && !place2.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
                {
                    player2Units.Add((place2.Unit, pos));
                }
            }
            
            Debug.Log($"Line {line}: Player 1 has {player1Units.Count} units, Player 2 has {player2Units.Count} units");
            
            // Process units in pairs (first with first, second with second, etc.)
            int maxUnits = Mathf.Max(player1Units.Count, player2Units.Count);
            
            for (int i = 0; i < maxUnits; i++)
            {
                GameUnitData unit1 = i < player1Units.Count ? player1Units[i].unit : null;
                GameUnitData unit2 = i < player2Units.Count ? player2Units[i].unit : null;
                
                if (unit1 != null && unit2 != null)
                {
                    // Both units exist, simultaneous combat
                    await ProcessPairedCombatAsync(gameState, unit1, unit2, 0, 1, line);
                }
                else if (unit1 != null)
                {
                    // Only player 1 unit exists
                    await ProcessSingleUnitAttackAsync(gameState, unit1, 0, line);
                }
                else if (unit2 != null)
                {
                    // Only player 2 unit exists
                    await ProcessSingleUnitAttackAsync(gameState, unit2, 1, line);
                }
                
                // Add delay between attacks in the same line
                await UniTask.DelayFrame(10);
            }
        }
        
        /// <summary>
        /// Processes simultaneous combat between two paired units
        /// </summary>
        private void ProcessPairedCombat(GameState gameState, GameUnitData unit1, GameUnitData unit2, int player1Index, int player2Index, int line)
        {
            Debug.Log($"Paired combat between players {player1Index} and {player2Index} at line {line}");
            
            var pendingDamage = new List<PendingDamage>();
            var player1State = gameState.GetState(player1Index);
            var player2State = gameState.GetState(player2Index);
            
            // Unit1 finds target in player2's line
            var unit1Target = FindTargetInLine(gameState, player2Index, line);
            if (unit1Target != null)
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit1,
                    Target = unit1Target.Unit,
                    TargetPlace = unit1Target,
                    TargetHero = null,
                    Amount = unit1.UnitState.Damage
                });
                
                if (unit1.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit1,
                        Target = unit1Target.Unit,
                        TargetPlace = unit1Target,
                        TargetHero = null,
                        Amount = unit1.UnitState.Damage
                    });
                }
            }
            else
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit1,
                    Target = null,
                    TargetPlace = null,
                    TargetHero = player2State.Hero,
                    Amount = unit1.UnitState.Damage
                });
                
                if (unit1.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit1,
                        Target = null,
                        TargetPlace = null,
                        TargetHero = player2State.Hero,
                        Amount = unit1.UnitState.Damage
                    });
                }
            }
            
            // Unit2 finds target in player1's line
            var unit2Target = FindTargetInLine(gameState, player1Index, line);
            if (unit2Target != null)
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit2,
                    Target = unit2Target.Unit,
                    TargetPlace = unit2Target,
                    TargetHero = null,
                    Amount = unit2.UnitState.Damage
                });
                
                if (unit2.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit2,
                        Target = unit2Target.Unit,
                        TargetPlace = unit2Target,
                        TargetHero = null,
                        Amount = unit2.UnitState.Damage
                    });
                }
            }
            else
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit2,
                    Target = null,
                    TargetPlace = null,
                    TargetHero = player1State.Hero,
                    Amount = unit2.UnitState.Damage
                });
                
                if (unit2.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit2,
                        Target = null,
                        TargetPlace = null,
                        TargetHero = player1State.Hero,
                        Amount = unit2.UnitState.Damage
                    });
                }
            }
            
            // Apply all damage simultaneously
            ApplyAllPendingDamage(pendingDamage);
            _ = ApplySecondaryEffectsAsync(gameState, pendingDamage);
        }

        private async UniTask ProcessPairedCombatAsync(GameState gameState, GameUnitData unit1, GameUnitData unit2, int player1Index, int player2Index, int line)
        {
            Debug.Log($"Paired combat between players {player1Index} and {player2Index} at line {line}");
            
            var pendingDamage = new List<PendingDamage>();
            var player1State = gameState.GetState(player1Index);
            var player2State = gameState.GetState(player2Index);
            
            // Play attack animations simultaneously
            var animationTasks = new List<UniTask>();
            
            var boardUnit1 = GetBoardUnitForUnit(gameState, unit1, player1Index);
            var boardUnit2 = GetBoardUnitForUnit(gameState, unit2, player2Index);
            
            if (boardUnit1 != null)
            {
                animationTasks.Add(boardUnit1.PlayAttackAnimation());
            }
            
            if (boardUnit2 != null)
            {
                animationTasks.Add(boardUnit2.PlayAttackAnimation());
            }
            
            // Wait for all animations to complete simultaneously
            if (animationTasks.Count > 0)
            {
                await UniTask.WhenAll(animationTasks);
            }
            
            // Unit1 finds target in player2's line
            var unit1Target = FindTargetInLine(gameState, player2Index, line);
            if (unit1Target != null)
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit1,
                    Target = unit1Target.Unit,
                    TargetPlace = unit1Target,
                    TargetHero = null,
                    Amount = unit1.UnitState.Damage
                });
                
                if (unit1.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit1,
                        Target = unit1Target.Unit,
                        TargetPlace = unit1Target,
                        TargetHero = null,
                        Amount = unit1.UnitState.Damage
                    });
                }
            }
            else
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit1,
                    Target = null,
                    TargetPlace = null,
                    TargetHero = player2State.Hero,
                    Amount = unit1.UnitState.Damage
                });
                
                if (unit1.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit1,
                        Target = null,
                        TargetPlace = null,
                        TargetHero = player2State.Hero,
                        Amount = unit1.UnitState.Damage
                    });
                }
            }
            
            // Unit2 finds target in player1's line
            var unit2Target = FindTargetInLine(gameState, player1Index, line);
            if (unit2Target != null)
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit2,
                    Target = unit2Target.Unit,
                    TargetPlace = unit2Target,
                    TargetHero = null,
                    Amount = unit2.UnitState.Damage
                });
                
                if (unit2.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit2,
                        Target = unit2Target.Unit,
                        TargetPlace = unit2Target,
                        TargetHero = null,
                        Amount = unit2.UnitState.Damage
                    });
                }
            }
            else
            {
                pendingDamage.Add(new PendingDamage {
                    Attacker = unit2,
                    Target = null,
                    TargetPlace = null,
                    TargetHero = player1State.Hero,
                    Amount = unit2.UnitState.Damage
                });
                
                if (unit2.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    pendingDamage.Add(new PendingDamage {
                        Attacker = unit2,
                        Target = null,
                        TargetPlace = null,
                        TargetHero = player1State.Hero,
                        Amount = unit2.UnitState.Damage
                    });
                }
            }
            
            // Apply all damage simultaneously
            ApplyAllPendingDamage(pendingDamage);
            await ApplySecondaryEffectsAsync(gameState, pendingDamage);
            
            // Wait for animations to complete
            await UniTask.DelayFrame(15);
        }
        
        /// <summary>
        /// Processes attack for a single unit when no opponent exists at the same position
        /// </summary>
        private void ProcessSingleUnitAttack(GameState gameState, GameUnitData attacker, int attackerPlayerIndex, int line)
        {
            int opponentIndex = 1 - attackerPlayerIndex;
            var opponentState = gameState.GetState(opponentIndex);
            
            Debug.Log($"Single unit attack from player {attackerPlayerIndex} at line {line}");
            
            // Find target in opponent's line
            var targetPlace = FindTargetInLine(gameState, opponentIndex, line);
            
            if (targetPlace != null)
            {
                // Target found, attack it
                Debug.Log($"Target found in line {line} for player {attackerPlayerIndex}");
                DealDamage(gameState, attacker, targetPlace, opponentState.Hero);
                
                // Handle double damage
                if (attacker.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(gameState, attacker, targetPlace, opponentState.Hero);
                }
            }
            else
            {
                // No target found, attack hero
                Debug.Log($"No targets in line {line}, player {attackerPlayerIndex} attacks hero");
                DealDamage(gameState, attacker, null, opponentState.Hero);
                
                // Handle double damage
                if (attacker.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(gameState, attacker, null, opponentState.Hero);
                }
            }
        }

        private async UniTask ProcessSingleUnitAttackAsync(GameState gameState, GameUnitData attacker, int attackerPlayerIndex, int line)
        {
            int opponentIndex = 1 - attackerPlayerIndex;
            var opponentState = gameState.GetState(opponentIndex);
            
            Debug.Log($"Single unit attack from player {attackerPlayerIndex} at line {line}");
            
            // Play attack animation
            var boardUnit = GetBoardUnitForUnit(gameState, attacker, attackerPlayerIndex);
            if (boardUnit != null)
            {
                await boardUnit.PlayAttackAnimation();
            }
            
            // Find target in opponent's line
            var targetPlace = FindTargetInLine(gameState, opponentIndex, line);
            
            if (targetPlace != null)
            {
                // Target found, attack it
                Debug.Log($"Target found in line {line} for player {attackerPlayerIndex}");
                DealDamage(gameState, attacker, targetPlace, opponentState.Hero);
                
                // Handle double damage
                if (attacker.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(gameState, attacker, targetPlace, opponentState.Hero);
                }
            }
            else
            {
                // No target found, attack hero
                Debug.Log($"No targets in line {line}, player {attackerPlayerIndex} attacks hero");
                DealDamage(gameState, attacker, null, opponentState.Hero);
                
                // Handle double damage
                if (attacker.UnitState.Skills.Contains(SkillId.DoubleDamage))
                {
                    DealDamage(gameState, attacker, null, opponentState.Hero);
                }
            }
            
            // Add delay after attack
            await UniTask.DelayFrame(10);
        }
        
        /// <summary>
        /// Finds the first living target in the specified player's line
        /// </summary>
        private GameBoardPlace FindTargetInLine(GameState gameState, int playerIndex, int line)
        {
            var playerState = gameState.GetState(playerIndex);
            
            for (int row = 0; row < 3; row++)
            {
                var place = playerState.Board.GetPlace(line, row);
                
                if (!place.IsEmpty && place.Unit.UnitState.Health > 0)
                {
                    return place;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Helper method to find BoardUnit by unit data
        /// </summary>
        private BoardUnit GetBoardUnitForUnit(GameState gameState, GameUnitData unit, int playerIndex)
        {
            var playerState = gameState.GetState(playerIndex);
            
            // Iterate through all board positions to find the matching unit
            for (int line = 0; line < 3; line++)
            {
                for (int row = 0; row < 3; row++)
                {
                    var place = playerState.Board.GetPlace(line, row);
                    if (!place.IsEmpty && place.Unit == unit)
                    {
                        return GameUI.Instance.GetBoardUnit(playerIndex, line, row);
                    }
                }
            }
            
            return null;
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
                    DealDamage(gameState, attacker, targetPlace, opponentState.Hero);
                    
                    // Found a target, so stop searching
                    return;
                }
            }
            
            // If no target found in the same line, damage the hero
            Debug.Log($"No targets in line {attackerLine}, attacking hero of player {opponentIndex}");
            DealDamage(gameState, attacker, null, opponentState.Hero);
        }
        
        private void DealDamage(GameState gameState, GameUnitData attacker, GameBoardPlace targetPlace, HeroState targetHero)
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
                
                // Check if target died and play death animation immediately
                if (targetUnit.UnitState.Health <= 0)
                {
                    // Find which player owns this target unit
                    int targetPlayerIndex = GetPlayerIndexForUnit(gameState, targetUnit);
                    
                    // Find the visual BoardUnit and play death animation
                    var boardUnit = GetBoardUnitByPlace(gameState, targetPlace, targetPlayerIndex);
                    if (boardUnit != null)
                    {
                        _ = boardUnit.PlayDeathAnimation(); // Fire and forget
                        // Remove from GameUI's dictionary immediately
                        GameUI.Instance?.RemoveBoardUnit(boardUnit);
                    }
                    
                    targetPlace.KillUnit(targetUnit);
                }
            }
        }
        
        /// <summary>
        /// Helper method to find BoardUnit by GameBoardPlace
        /// </summary>
        private BoardUnit GetBoardUnitByPlace(GameState gameState, GameBoardPlace targetPlace, int playerIndex)
        {
            var playerState = gameState.GetState(playerIndex);
            
            // Find the position of this place on the board
            for (int line = 0; line < 3; line++)
            {
                for (int row = 0; row < 3; row++)
                {
                    var place = playerState.Board.GetPlace(line, row);
                    if (place == targetPlace)
                    {
                        return GameUI.Instance.GetBoardUnit(playerIndex, line, row);
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Helper method to determine which player owns a unit
        /// </summary>
        private int GetPlayerIndexForUnit(GameState gameState, GameUnitData unit)
        {
            for (int playerIndex = 0; playerIndex < 2; playerIndex++)
            {
                var playerState = gameState.GetState(playerIndex);
                for (int line = 0; line < 3; line++)
                {
                    for (int row = 0; row < 3; row++)
                    {
                        var place = playerState.Board.GetPlace(line, row);
                        if (!place.IsEmpty && place.Unit == unit)
                        {
                            return playerIndex;
                        }
                    }
                }
            }
            
            return -1; // Not found
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
        private void ApplySecondaryEffects(GameState gameState, List<PendingDamage> pendingDamage)
        {
            _ = ApplySecondaryEffectsAsync(gameState, pendingDamage);
        }

        private async UniTask ApplySecondaryEffectsAsync(GameState gameState, List<PendingDamage> pendingDamage)
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
                    // Find which player owns this target unit
                    int targetPlayerIndex = GetPlayerIndexForUnit(gameState, damage.Target);
                    
                    // Find the visual BoardUnit and play death animation
                    var boardUnit = GetBoardUnitByPlace(gameState, damage.TargetPlace, targetPlayerIndex);
                    if (boardUnit != null)
                    {
                        await boardUnit.PlayDeathAnimation();
                        // Remove from GameUI's dictionary immediately
                        GameUI.Instance?.RemoveBoardUnit(boardUnit);
                    }
                    
                    damage.TargetPlace.KillUnit(damage.Target);
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
