using Game.Scripts.Data.Core.State;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// A no-op effect used as a fallback when an effect isn't implemented
    /// </summary>
    public class NoEffect : BaseCardEffect
    {
        public NoEffect() : base(EffectType.PassiveAbility)
        {
        }

        public override bool ApplyEffect(
            GameState gameState,
            int sourcePlayerIndex,
            int sourceLine,
            int sourceRow)
        {
            // Do nothing
            return true;
        }
    }
}