using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Interface for all card effects
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// Apply the effect
        /// </summary>
        /// <param name="gameState">Current game state</param>
        /// <param name="sourcePlayerIndex">Index of the player who played the card</param>
        /// <param name="sourceLine">Board line where the card was played (for unit cards) or targeted (for spell cards)</param>
        /// <param name="sourceRow">Board row where the card was played (for unit cards) or targeted (for spell cards)</param>
        /// <returns>True if the effect was applied successfully</returns>
        bool ApplyEffect(
            GameState gameState, 
            int sourcePlayerIndex, 
            int sourceLine, 
            int sourceRow);
            
        /// <summary>
        /// Returns the effect type for ordering during battle resolution
        /// </summary>
        EffectType GetEffectType();
    }
    
    /// <summary>
    /// Enum to determine the order of effect resolution
    /// </summary>
    public enum EffectType
    {
        // Battle resolution order from the game requirements
        PositiveEffect = 0,  // Buffs, heals
        NegativeEffect = 1,  // Damage, debuffs
        PassiveAbility = 2,  // "At start of turn" triggers
        Combat = 3           // Combat phase
    }
}