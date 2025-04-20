using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that grants armor to a unit, reducing damage taken
    /// </summary>
    public class ArmorEffect : BaseCardEffect
    {
        public ArmorEffect() : base(EffectType.PositiveEffect)
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
                Debug.LogWarning("Cannot apply Armor effect: no unit at the target position");
                return false;
            }
            
            // Add the Armor skill
            if (!boardPlace.Unit.UnitState.Skills.Contains(SkillId.Armor))
            {
                boardPlace.Unit.UnitState.AddSkill(SkillId.Armor);
                Debug.Log("Added Armor skill to unit");
                return true;
            }
            
            // Unit already has the skill
            Debug.Log("Unit already has Armor skill");
            return false;
        }
    }
}