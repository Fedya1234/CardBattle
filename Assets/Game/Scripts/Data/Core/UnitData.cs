using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class UnitData
    {
        public event Action<UnitData> EventDataChanged;
        
        public int Health;
        public int Damage;
        public int ManaCost;
        public List<SkillId> Skills = new List<SkillId>();
        
        public UnitData(UnitStaticData staticData)
        {
            Health = staticData.Health;
            Damage = staticData.Damage;
            ManaCost = staticData.ManaCost;
            Skills = new List<SkillId>(staticData.Skills);
        }
        
        public UnitData(UnitData unitData)
        {
            Health = unitData.Health;
            Damage = unitData.Damage;
            ManaCost = unitData.ManaCost;
            Skills = new List<SkillId>(unitData.Skills);
        }
        
        public void AddHealth(int health)
        {
            Health += health;
            if (Health < 0)
            {
                Health = 0;
            }
            EventDataChanged?.Invoke(this);
        }
        
        public void AddDamage(int damage)
        {
            Damage += damage;
            if (Damage < 0)
            {
                Damage = 0;
            }
            EventDataChanged?.Invoke(this);
        }
        
        public void SetManaCost(int manaCost)
        {
            ManaCost = manaCost;
            if (ManaCost < 0) 
                ManaCost = 0;
            
            EventDataChanged?.Invoke(this);
        }
        
        public void AddSkill(SkillId skillId)
        {
            Skills.Add(skillId);
            EventDataChanged?.Invoke(this);
        }
        
        public void RemoveSkill(SkillId skillId)
        {
            if (Skills.Remove(skillId) == false)
                return;

            EventDataChanged?.Invoke(this);
        }
    }
}