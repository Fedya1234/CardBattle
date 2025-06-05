using System;
using System.Collections.Generic;
using Game.Scripts.Data.Core;
using Game.Scripts.Data.Core.Units;
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
        [SerializeField] private Dictionary<CardId, CardStaticData> _cards;
        [SerializeField] private Dictionary<HeroId, HeroData> _heroes;
        [SerializeField] private Dictionary<HeroMagicId, HeroMagicData> _heroMagic;
        [SerializeField] private int[][] _heroMagicGradeCosts;
        [SerializeField] private int[] _playerHealthByLevel;

        private void OnValidate()
        {
            foreach (var (id, data) in _cards)
            {
                data.Id = id;
            }
            
            foreach (var (id, data) in _units)
            {
                foreach (var unitStaticData in data.DataByLevel)
                {
                    if (unitStaticData.Skills == null)
                        unitStaticData.Skills = new SkillId[] {};
                }
            }
        }
        
        
        public int GetHeroMagicCost(int magicLevel, int grade)
        {
            if (_heroMagicGradeCosts == null)
            {
                Debug.LogError("HeroMagicGradeCosts data not initialized in StaticData");
                return 0;
            }
            if (_heroMagicGradeCosts.Length == 0)
            {
                Debug.LogError("HeroMagicGradeCosts data not set in StaticData");
                return 0;
            }
            if (magicLevel >= _heroMagicGradeCosts.Length)
            {
                Debug.LogError($"HeroMagicGradeCosts data not found for MagicLevel: {magicLevel}");
                return _heroMagicGradeCosts[^1][grade];
            }
            
            if (grade >= _heroMagicGradeCosts[magicLevel].Length)
            {
                Debug.LogError($"HeroMagicGradeCosts data not found for Grade: {grade}");
                return _heroMagicGradeCosts[magicLevel][^1];
            }
            
            if (_heroMagicGradeCosts[magicLevel][grade] == 0)
            {
                Debug.LogError($"HeroMagicGradeCosts data not set for MagicLevel: {magicLevel} and Grade: {grade}");
                return 0;
            }
            return _heroMagicGradeCosts[magicLevel][grade];
        }

        public HeroMagicData GetHeroMagicData(HeroMagicId magicId)
        {
            if (_heroMagic == null)
            {
                Debug.LogError("HeroMagic data not initialized in StaticData");
                return null;
            }

            if (!_heroMagic.TryGetValue(magicId, out var magicData))
            {
                Debug.LogError($"HeroMagic data not found for MagicId: {magicId}");
                return null;
            }

            return magicData;
        }

        public UnitStaticData GetUnitStaticData(UnitData data)
        {
            var cardLevelData = GetUnitLevelData(data.Id);
            return cardLevelData.GetDataByLevel(data.Level);
        }

        public CardStaticData GetCardStaticData(CardId cardId)
        {
            if (_cards == null)
            {
                Debug.LogError("Cards data not initialized in StaticData");
                return null;
            }

            if (!_cards.TryGetValue(cardId, out var cardData))
            {
                Debug.LogError($"Card data not found for CardId: {cardId}");
                return null;
            }

            return cardData;
        }

        public int GetCardManaCost(CardId cardId)
        {
            var cardData = GetCardStaticData(cardId);
            return cardData?.ManaCost ?? 1; // Default to 1 if data not found
        }
        
        public HeroData GetHeroData(HeroId heroId)
        {
            if (_heroes == null)
            {
                Debug.LogError("Heroes data not initialized in StaticData");
                return new HeroData(20);
            }
            
            if (!_heroes.TryGetValue(heroId, out var heroData))
            {
                Debug.LogError($"HeroData not found for HeroId: {heroId}");
                return new HeroData(20);
            }
            
            return heroData;
        }
        
        public int GetHeroHealth(HeroId heroId)
        {
            var heroData = GetHeroData(heroId);
            return heroData.Health;
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

        private UnitStaticLevelData GetUnitLevelData(UnitId unitId)
        {
            if (_units.ContainsKey(unitId) == false)
            {
                Debug.LogError($"CardLevelData not found for {unitId}");
                return new UnitStaticLevelData();
            }
            return _units[unitId];
        }
    }
}