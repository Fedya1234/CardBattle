using System.Collections.Generic;
using Game.Scripts.Data.Core;
using Game.Scripts.Data.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Data.Static
{
    [CreateAssetMenu(menuName = "SO/StaticData", fileName = "StaticData")]
    public class StaticData : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<UnitId, UnitStaticLevelData> _units;
        [SerializeField] private int[] _playerHealthByLevel;
        
        public UnitStaticLevelData GetUnitLevelData(UnitId unitId)
        {
            if (_units.ContainsKey(unitId) == false)
            {
                Debug.LogError($"CardLevelData not found for {unitId}");
                return new UnitStaticLevelData();
            }
            return _units[unitId];
        }

        public UnitStaticData GetUnitStaticData(UnitId unitId, int level)
        {
            var cardLevelData = GetUnitLevelData(unitId);
            return cardLevelData.GetDataByLevel(level);
        }

        public int PlayerHealth(int level)
        {
            if (_playerHealthByLevel.Length == 0)
            {
                Debug.LogError($"HealthByLevel not set");
                return 0;
            }

            if (level >= _playerHealthByLevel.Length)
            {
                Debug.LogWarning($"HealthByLevel not found for level {level}");
                return _playerHealthByLevel[_playerHealthByLevel.Length - 1];
            }

            return _playerHealthByLevel[level];
        }
    }
}