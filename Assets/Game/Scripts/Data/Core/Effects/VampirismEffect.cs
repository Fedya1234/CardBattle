using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that grants vampirism to a unit, allowing it to heal when dealing damage
    /// </summary>
    public class VampirismEffect : BaseCardEffect
    {
        public VampirismEffect() : base(EffectType.PositiveEffect)
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
                Debug.LogWarning("Cannot apply Vampirism effect: no unit at the target position");
                return false;
            }
            
            // Add the Vampirism skill
            if (!boardPlace.Unit.UnitState.Skills.Contains(SkillId.Vampire))
            {
                boardPlace.Unit.UnitState.AddSkill(SkillId.Vampire);
                Debug.Log("Added Vampirism skill to unit");
                return true;
            }
            
            // Unit already has the skill
            Debug.Log("Unit already has Vampirism skill");
            return false;
        }
    }
}