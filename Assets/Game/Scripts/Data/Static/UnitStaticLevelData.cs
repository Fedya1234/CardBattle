using System;
using Game.Scripts.Data.Core;
using UnityEngine;

namespace Game.Scripts.Data.Static
{
    [Serializable]
    public class UnitStaticLevelData
    {
        public UnitStaticData[] DataByLevel;
        
        public UnitStaticData GetDataByLevel(int level)
        {
            if (level >= DataByLevel.Length)
            {
                Debug.LogWarning($"CardData not found for level {level}");
                return DataByLevel[DataByLevel.Length - 1];
            }
            
            if (DataByLevel.Length == 0)
            {
                Debug.LogError($"Static Data Not Set for CardLevelData");
                return new UnitStaticData();
            }
            
            return DataByLevel[level];
        }
    }
}