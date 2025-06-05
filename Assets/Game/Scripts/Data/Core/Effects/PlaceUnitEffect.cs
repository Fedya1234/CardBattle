using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect for placing a unit on the board
    /// </summary>
    public class PlaceUnitEffect : BaseCardEffect
    {
        private readonly UnitData _unitData;
        private readonly int _manaCost;
        
        public PlaceUnitEffect(UnitData unitData, int manaCost) : base(EffectType.PositiveEffect)
        {
            _unitData = unitData;
            _manaCost = manaCost;
        }

        public override bool ApplyEffect(
            GameState gameState, 
            int sourcePlayerIndex, 
            int sourceLine, 
            int sourceRow)
        {
            // Get the player state
            var playerState = gameState.GetState(sourcePlayerIndex);
            
            // Check if the target position is empty
            var boardPlace = playerState.Board.GetPlace(sourceLine, sourceRow);
            if (!boardPlace.IsEmpty)
            {
                Debug.LogWarning($"Cannot place unit at {sourceLine},{sourceRow} because it's not empty");
                return false;
            }
            
            // Check if player has enough mana
            if (playerState.Hero.Mana < _manaCost)
            {
                Debug.LogWarning($"Not enough mana to place unit: {playerState.Hero.Mana} < {_manaCost}");
                return false;
            }
            
            // Consume mana
            playerState.Hero.ChangeMana(-_manaCost);
            
            // Create the unit
            // In a real implementation, we'd get the static data from a service
            // For now, let's create a simple implementation
            var staticData = new UnitStaticData
            {
                Health = 2,
                Damage = 1,
                Skills = new [] { SkillId.None }
            };
            
            var visualData = new UnitVisual(); // Placeholder
            var gameUnit = new GameUnitData(staticData, visualData, _unitData);
            
            // Place the unit
            boardPlace.PlaceUnit(gameUnit);
            
            return true;
        }
    }
}