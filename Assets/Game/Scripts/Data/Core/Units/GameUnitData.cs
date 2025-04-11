using System;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;

namespace Game.Scripts.Data.Core.Units
{
    [Serializable]
    public class GameUnitData
    {
        public readonly UnitData UnitData;
        
        public readonly UnitVisual Visual;

        public readonly UnitState UnitState;
        
        public GameUnitData(UnitStaticData unitStaticData, UnitVisual visualData, UnitData unitData)
        {
            Visual = visualData;
            UnitData = unitData;
            UnitState = new UnitState(unitStaticData);
        }
    }
}