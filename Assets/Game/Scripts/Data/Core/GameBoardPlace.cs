using System;
using System.Collections.Generic;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class GameBoardPlace
    {
        public List<PlaceMarkId> PlaceMarks = new List<PlaceMarkId>();
        public GameUnitData Unit;
        public List<GameUnitData> DeadUnits = new List<GameUnitData>();
        
        public bool IsEmpty => Unit == default;
        public bool IsCanRevive => DeadUnits.Count > 0 && Unit == default;

        public void PlaceMark(PlaceMarkId mark)
        {
            PlaceMarks.Add(mark);
        }
        
        public void RemoveMark(PlaceMarkId mark)
        {
            PlaceMarks.Remove(mark);
        }
        
        public void PlaceUnit(GameUnitData unit)
        {
            Unit = unit;
        }
        
        public void RemoveUnit()
        {
            Unit = default;
        }
        
        public void KillUnit(GameUnitData unit)
        {
            Unit = default;
            DeadUnits.Add(unit);
        }

        public void ReviveLastUnit()
        {
            if (IsCanRevive == false)
                return;
            
            var unit = DeadUnits[^1];
            DeadUnits.Remove(unit);
            unit.UnitState.Reset();
            
            PlaceUnit(unit);
        }
    }
}