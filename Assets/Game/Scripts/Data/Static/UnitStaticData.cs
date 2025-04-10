using System;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Static
{
    [Serializable]
    public class UnitStaticData
    {
        public int Health;
        public int Damage;
        public int ManaCost;
        public SkillId[] Skills;
    }
}