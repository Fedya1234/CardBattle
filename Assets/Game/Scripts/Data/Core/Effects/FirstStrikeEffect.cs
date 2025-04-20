using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that grants first strike to a unit, allowing it to attack before others
    /// </summary>
    public class FirstStrikeEffect : BaseCardEffect
    {
        public FirstStrikeEffect() : base(EffectType.PositiveEffect)
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
                Debug.LogWarning("Cannot apply First Strike effect: no unit at the target position");
                return false;
            }
            
            // Add the First Strike skill
            if (!boardPlace.Unit.UnitState.Skills.Contains(SkillId.FirstHit))
            {
                boardPlace.Unit.UnitState.AddSkill(SkillId.FirstHit);
                Debug.Log("Added First Strike skill to unit");
                return true;
            }
            
            // Unit already has the skill
            Debug.Log("Unit already has First Strike skill");
            return false;
        }
    }
}