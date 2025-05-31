using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core;
using Game.Scripts.Data.Core.Effects;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Core
{
  /// <summary>
  /// Handles the battle resolution phase based on both players' moves
  /// </summary>
  public class BattleResolver
  {
    private readonly CardEffectFactory _effectFactory;
        
    public BattleResolver()
    {
      _effectFactory = new CardEffectFactory();
    }
        
    /// <summary>
    /// Resolves a battle round
    /// </summary>
    /// <param name="gameState">Current game state</param>
    /// <param name="player1Move">Move from player 1</param>
    /// <param name="player2Move">Move from player 2</param>
    /// <returns>True if the game has ended, false if it continues</returns>
    public async UniTask<bool> ResolveBattle(GameState gameState, PlayerMove player1Move, PlayerMove player2Move)
    {
      // Apply card effects in the proper order according to the game rules
            
      // 1. Process card placements and effects
      await ProcessPlayedCards(gameState, player1Move, player2Move);
            
      // 2. Execute battle resolution in phases
      await ExecuteBattlePhases(gameState);
            
      // 3. Analyze the current game state advantage
      AnalyzeGameState(gameState);
            
      // 4. Check for win conditions
      return CheckWinConditions(gameState);
    }

    private async UniTask ProcessPlayedCards(GameState gameState, PlayerMove player1Move, PlayerMove player2Move)
    {
      // Combine moves for processing in the correct order
      var allCardMoves = new List<(CardMove move, int playerIndex)>();
            
      foreach (var move in player1Move.Cards)
      {
        allCardMoves.Add((move, 0)); // Player 1 index is 0
      }
            
      foreach (var move in player2Move.Cards)
      {
        allCardMoves.Add((move, 1)); // Player 2 index is 1
      }
            
      // Process cards by mana cost - higher cost cards go first
      // This simulates more powerful cards having initiative
      var orderedMoves = allCardMoves.OrderByDescending(x => x.move.Card.GetManaCost()).ToList();
            
      // Apply each card effect
      foreach (var (move, playerIndex) in orderedMoves)
      {
        // Get player state
        var playerState = gameState.GetState(playerIndex);
        var cardId = move.Card.Id;
                
        // Try to get the card data from static data
        ICardEffect cardEffect = null;

        var cardVisual = VisualService.GetCardTypeId(cardId);
        // Determine effect based on card type
        switch (cardVisual)
        {
          case CardTypeId.Unit:
            // Example: Unit card
            var unitData = new Data.Core.Units.UnitData
            {
              Id = Data.Enums.UnitId.Unit1,
              Level = move.Card.Level
            };
            cardEffect = _effectFactory.CreateUnitPlacementEffect(cardId, unitData);
            break;
                
          case CardTypeId.Magic:
            // Example: Buff spell - use magic effect factory
            cardEffect = _effectFactory.CreateMagicEffect(cardId);
            break;
                
          default:
            Debug.LogWarning($"Unknown card ID: {cardId}");
            cardEffect = new NoEffect();
            break;
        }
                
        if (cardEffect != null)
        {
          bool success = cardEffect.ApplyEffect(gameState, playerIndex, move.Line, move.Row);
          if (success)
          {
            Debug.Log($"Player {playerIndex} successfully played card {cardId} on {move.Line},{move.Row}");
          }
          else
          {
            Debug.LogWarning($"Player {playerIndex} failed to play card {cardId} on {move.Line},{move.Row}");
          }
                
          // Add delay for visual feedback (not necessary in this implementation)
          await UniTask.Delay(100);
        }
      }
            
      // Process burned cards (cards discarded to gain mana)
      ProcessBurnedCardsForMana(gameState, player1Move, 0);
      ProcessBurnedCardsForMana(gameState, player2Move, 1);
    }

    /// <summary>
    /// Process cards that were burned to gain mana
    /// </summary>
    private void ProcessBurnedCardsForMana(GameState gameState, PlayerMove playerMove, int playerIndex)
    {
      if (playerMove.BurnedForManaCard != null)
      {
        var playerState = gameState.GetState(playerIndex);
                
        // Add the card to discard pile
        playerState.Cards.Discard.Add(playerMove.BurnedForManaCard);
                
        // Remove from hand (already done in UI/controller, but ensure it's removed)
        if (playerState.Cards.HandCards.Contains(playerMove.BurnedForManaCard))
        {
          playerState.Cards.HandCards.Remove(playerMove.BurnedForManaCard);
        }
                
        // Add mana
        playerState.Hero.ChangeMana(1);
                
        Debug.Log($"Player {playerIndex} burned card {playerMove.BurnedForManaCard.Id} for +1 mana");
      }
    }

    private async UniTask ExecuteBattlePhases(GameState gameState)
    {
      // Following the battle resolution order from the game requirements:
            
      // 1. Positive spell effects (buffs, heals)
      await ApplyEffectsOfType(gameState, EffectType.PositiveEffect);
            
      // 2. Negative spell effects (damage, debuffs)
      await ApplyEffectsOfType(gameState, EffectType.NegativeEffect);
            
      // 3. Passive abilities that trigger "At start of turn"
      await ApplyEffectsOfType(gameState, EffectType.PassiveAbility);
            
      // 4. Combat phase - units fight
      await ApplyEffectsOfType(gameState, EffectType.Combat);
    }
        
    private async UniTask ApplyEffectsOfType(GameState gameState, EffectType effectType)
    {
      ICardEffect effect = null;
            
      // Create an appropriate effect based on the type
      switch (effectType)
      {
        case EffectType.Combat:
          effect = new CombatEffect();
          break;
                    
        // Other effect types would be handled by individual cards
        // and are applied during the ProcessPlayedCards phase
      }
            
      if (effect != null)
      {
        // For global effects like combat, we just pass 0,0,0 as positions
        // The effect itself will iterate over the whole board
        effect.ApplyEffect(gameState, 0, 0, 0);
                
        // Add delay for visual feedback (not necessary in this implementation)
        await UniTask.Delay(100);
      }
            
      // Apply passive effects on units
      ApplyPassiveEffects(gameState, effectType);
    }
        
    private void ApplyPassiveEffects(GameState gameState, EffectType effectType)
    {
      // Apply passive effects for all units on the board that match the current phase
      for (int playerIndex = 0; playerIndex < 2; playerIndex++)
      {
        var playerState = gameState.GetState(playerIndex);
                
        // Iterate through all board positions
        for (int line = 0; line < 3; line++)
        {
          for (int row = 0; row < 3; row++)
          {
            var place = playerState.Board.GetPlace(line, row);
                    
            if (!place.IsEmpty)
            {
              // Check for passive abilities that match the current phase
              foreach (var skill in place.Unit.UnitState.Skills)
              {
                // Only process relevant skills for the current phase
                if (ShouldProcessSkillInPhase(skill, effectType))
                {
                  var skillEffect = _effectFactory.CreateSkillEffect(skill);
                  skillEffect.ApplyEffect(gameState, playerIndex, line, row);
                }
              }
                      
              // Process board position-specific effects
              ApplyPositionBasedEffects(gameState, playerIndex, line, row, place, effectType);
            }
          }
        }
      }
    }
        
    /// <summary>
    /// Apply effects based on unit's position on the board
    /// </summary>
    private void ApplyPositionBasedEffects(
      GameState gameState, 
      int playerIndex, 
      int line, 
      int row, 
      GameBoardPlace place,
      EffectType effectType)
    {
      // Skip if not in the passive ability phase or if the place is empty
      if (effectType != EffectType.PassiveAbility || place.IsEmpty)
        return;
                
      var unit = place.Unit;
            
      // Front row effects (row 0)
      if (row == 0)
      {
        // Example: Units in the front row might get extra defense
        if (unit.UnitState.Skills.Contains(SkillId.EndTurnHealth_1))
        {
          Debug.Log("Front row unit heals at end of turn due to position");
          unit.UnitState.AddHealth(1);
        }
      }
            
      // Back row effects (row 2)
      if (row == 2)
      {
        // Example: Units in the back row might get a damage boost for ranged attacks
        // Nothing implemented yet
      }
    }
        
    private bool ShouldProcessSkillInPhase(SkillId skill, EffectType phase)
    {
      // Determine if a skill should be processed in the current phase
      switch (skill)
      {
        case SkillId.EndTurnHealth_1:
          return phase == EffectType.PassiveAbility;
                    
        // Other passive skills would be handled here
                    
        default:
          return false;
      }
    }
        
    private bool CheckWinConditions(GameState gameState)
    {
      // Check if any player has 0 or less health
      bool player1Dead = gameState.GetState(0).Hero.Health <= 0;
      bool player2Dead = gameState.GetState(1).Hero.Health <= 0;
            
      if (player1Dead || player2Dead)
      {
        if (player1Dead && player2Dead)
        {
          Debug.Log("Game ended in a draw!");
        }
        else if (player1Dead)
        {
          Debug.Log("Player 2 wins!");
        }
        else
        {
          Debug.Log("Player 1 wins!");
        }
                
        return true; // Game has ended
      }
            
      return false; // Game continues
    }

    /// <summary>
    /// Analyzes the current game state to determine which player has an advantage
    /// </summary>
    /// <param name="gameState">The current game state</param>
    /// <returns>A value between -1.0 and 1.0 where negative means player 2 advantage, 
    /// positive means player 1 advantage, and 0 is balanced</returns>
    public float AnalyzeGameState(GameState gameState)
    {
      var player1 = gameState.GetState(0);
      var player2 = gameState.GetState(1);
            
      float advantage = 0.0f;
            
      // Compare health directly
      float healthDiff = player1.Hero.Health - player2.Hero.Health;
      advantage += (healthDiff / 20f) * 0.4f; // Health is 40% of advantage
            
      // Compare board presence (units on the field)
      int player1Units = CountUnitsOnBoard(player1.Board);
      int player2Units = CountUnitsOnBoard(player2.Board);
      advantage += (player1Units - player2Units) * 0.1f; // Each unit difference is 10% advantage
            
      // Compare unit strength
      int player1Power = CalculateTotalUnitPower(player1.Board);
      int player2Power = CalculateTotalUnitPower(player2.Board);
      if (player1Power + player2Power > 0) // Avoid division by zero
      {
        float powerDiff = (float)(player1Power - player2Power) / (player1Power + player2Power);
        advantage += powerDiff * 0.3f; // Unit power is 30% of advantage
      }
            
      // Compare cards in hand
      int player1HandSize = player1.Cards.HandCards.Count;
      int player2HandSize = player2.Cards.HandCards.Count;
      advantage += (player1HandSize - player2HandSize) * 0.05f; // Each card difference is 5% advantage
            
      // Compare mana
      advantage += (player1.Hero.Mana - player2.Hero.Mana) * 0.05f; // Each mana difference is 5% advantage
            
      // Clamp to -1.0 to 1.0 range
      advantage = Mathf.Clamp(advantage, -1.0f, 1.0f);
            
      // Log the advantage
      string advantageText;
      if (advantage > 0.5f)
        advantageText = $"Player 1 has a strong advantage ({advantage:F2})";
      else if (advantage > 0.2f)
        advantageText = $"Player 1 has a slight advantage ({advantage:F2})";
      else if (advantage < -0.5f)
        advantageText = $"Player 2 has a strong advantage ({-advantage:F2})";
      else if (advantage < -0.2f)
        advantageText = $"Player 2 has a slight advantage ({-advantage:F2})";
      else
        advantageText = $"The game is balanced ({advantage:F2})";
                
      Debug.Log(advantageText);
            
      return advantage;
    }
        
    /// <summary>
    /// Counts the number of units on a player's board
    /// </summary>
    private int CountUnitsOnBoard(BoardState board)
    {
      int count = 0;
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          if (!board.GetPlace(line, row).IsEmpty)
            count++;
        }
      }
      return count;
    }
        
    /// <summary>
    /// Calculates the total power (health + damage) of all units on a player's board
    /// </summary>
    private int CalculateTotalUnitPower(BoardState board)
    {
      int power = 0;
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var place = board.GetPlace(line, row);
          if (!place.IsEmpty)
          {
            var unit = place.Unit;
            power += unit.UnitState.Health + unit.UnitState.Damage;
                        
            // Add bonus for valuable skills
            foreach (var skill in unit.UnitState.Skills)
            {
              switch (skill)
              {
                case SkillId.DoubleDamage:
                  power += 3;
                  break;
                case SkillId.FirstHit:
                  power += 2;
                  break;
                case SkillId.Vampire:
                  power += 2;
                  break;
                case SkillId.AntiMagic:
                  power += 2;
                  break;
                case SkillId.Armor:
                  power += 1;
                  break;
              }
            }
          }
        }
      }
      return power;
    }
  }
}