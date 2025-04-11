using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class UnitState
    {
        public event Action<UnitState> EventDataChanged;
        
        public int Health;
        public int Damage;
        public List<SkillId> Skills = new List<SkillId>();
        
        private UnitStaticData _staticData;
        
        public UnitState(UnitStaticData staticData)
        {
            _staticData = staticData;
            Reset();
        }

        public void Reset()
        {
            Health = _staticData.Health;
            Damage = _staticData.Damage;
            Skills = new List<SkillId>(_staticData.Skills);
            
            EventDataChanged?.Invoke(this);
        }
        public UnitState(UnitState unitState)
        {
            Health = unitState.Health;
            Damage = unitState.Damage;
            Skills = new List<SkillId>(unitState.Skills);
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