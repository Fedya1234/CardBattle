using Game.Scripts.Data.Core.State;
using UnityEngine;

namespace Game.Scripts.Data.Core.Effects
{
    /// <summary>
    /// Effect that heals a unit at the end of each turn
    /// </summary>
    public class EndTurnHealEffect : BaseCardEffect
    {
        private readonly int _healAmount;
        
        public EndTurnHealEffect(int healAmount) : base(EffectType.PassiveAbility)
        {
            _healAmount = healAmount;
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
                Debug.LogWarning("Cannot apply End Turn Heal effect: no unit at the target position");
                return false;
            }
            
            // Heal the unit
            boardPlace.Unit.UnitState.AddHealth(_healAmount);
            Debug.Log($"Healed unit for {_healAmount} at end of turn");
            
            return true;
        }
    }
}