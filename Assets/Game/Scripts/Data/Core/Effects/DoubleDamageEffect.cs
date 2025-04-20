using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that grants double damage to a unit
    /// </summary>
    public class DoubleDamageEffect : BaseCardEffect
    {
        public DoubleDamageEffect() : base(EffectType.PositiveEffect)
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
                Debug.LogWarning("Cannot apply DoubleDamage effect: no unit at the target position");
                return false;
            }
            
            // Add the DoubleDamage skill
            if (!boardPlace.Unit.UnitState.Skills.Contains(SkillId.DoubleDamage))
            {
                boardPlace.Unit.UnitState.AddSkill(SkillId.DoubleDamage);
                Debug.Log("Added DoubleDamage skill to unit");
                return true;
            }
            
            // Unit already has the skill
            Debug.Log("Unit already has DoubleDamage skill");
            return false;
        }
    }
}