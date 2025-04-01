using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class CardData
    {
        public int Health;
        public int Damage;
        public int ManaCost;
        public List<SkillId> Skills = new List<SkillId>();
    }
}