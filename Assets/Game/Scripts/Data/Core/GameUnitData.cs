using System;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class GameUnitData
    {
        public event Action<GameUnitData> EventDataChanged;
        
        public readonly UnitId Id;
        public readonly int Level;
        public readonly RarityId Rarity;
        public readonly RaceId Race;
        public readonly ClassId Class;

        public UnitData Data;
        
        public GameUnitData(UnitStaticData unitData, UnitVisual visualData, UnitId id, int level)
        {
            Rarity = visualData.Rarity;
            Race = visualData.Race;
            Class = visualData.Class;
            Id = id;
            Level = level;
            Data = new UnitData(unitData);
        }
    }
}