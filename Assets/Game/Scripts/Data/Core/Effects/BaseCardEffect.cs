using Game.Scripts.Data.Core.State;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Base class for all card effects
    /// </summary>
    public abstract class BaseCardEffect : ICardEffect
    {
        /// <summary>
        /// The type of effect, which determines order of execution in battle phase
        /// </summary>
        protected EffectType effectType;

        protected BaseCardEffect(EffectType effectType)
        {
            this.effectType = effectType;
        }
        
        public abstract bool ApplyEffect(
            GameState gameState, 
            int sourcePlayerIndex, 
            int sourceLine, 
            int sourceRow);

        public EffectType GetEffectType()
        {
            return effectType;
        }
        
        /// <summary>
        /// Helper method to get the target player index
        /// </summary>
        /// <param name="sourcePlayerIndex">Source player index</param>
        /// <returns>Target (opponent) player index</returns>
        protected int GetTargetPlayerIndex(int sourcePlayerIndex)
        {
            return sourcePlayerIndex == 0 ? 1 : 0;
        }
        
        /// <summary>
        /// Helper method to get the opponent's matching board position
        /// </summary>
        /// <param name="sourceRow">Row in source player's board</param>
        /// <returns>Matching row in opponent's board</returns>
        protected int GetOpponentMatchingRow(int sourceRow)
        {
            // In a 3x3 grid, we can use this formula to get the mirrored position
            return sourceRow;
        }
    }
}