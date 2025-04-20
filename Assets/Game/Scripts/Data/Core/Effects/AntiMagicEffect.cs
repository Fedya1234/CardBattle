using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that grants anti-magic to a unit, making it immune to magic effects
    /// </summary>
    public class AntiMagicEffect : BaseCardEffect
    {
        public AntiMagicEffect() : base(EffectType.PositiveEffect)
        {
        }

        public override bool ApplyEffect(
            GameState gameState,
            int sourcePlayerIndex,
            int sourceLine,
            int sourceRow)
        {
            // Get the target unit
            var playerState = gameState.GetState(sourcePlayerIndex);
            var boardPlace = playerState.Board.GetPlace(sourceLine, sourceRow);
            
            if (boardPlace.IsEmpty)
            {
                Debug.LogWarning("Cannot apply Anti-Magic effect: no unit at the target position");
                return false;
            }
            
            // Add the Anti-Magic skill
            if (!boardPlace.Unit.UnitState.Skills.Contains(SkillId.AntiMagic))
            {
                boardPlace.Unit.UnitState.AddSkill(SkillId.AntiMagic);
                Debug.Log("Added Anti-Magic skill to unit");
                return true;
            }
            
            // Unit already has the skill
            Debug.Log("Unit already has Anti-Magic skill");
            return false;
        }
    }
}