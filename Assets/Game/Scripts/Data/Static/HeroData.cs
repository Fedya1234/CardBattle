using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using UnityEngine;

namespace Game.Scripts.Data.Static
{
    [Serializable]
    public class HeroData
    {
        [field: SerializeField] public int Health { get; private set; }
        [field: SerializeField] public List<HeroMagicId> Magics { get; private set; }
        
        public HeroData(int health)
        {
            Health = health;
        }
        
        public HeroMagicId GetMagic(int level)
        {
            return level < Magics.Count ? Magics[level] : HeroMagicId.Default;
        }

        public int GetMagicLevel(HeroMagicId magicId)
        {
            var index = Magics.IndexOf(magicId);
            return index != -1 ? index : 0;
        }
        
        public bool IsHaveMagic(HeroMagicId magicId)
        {
            return Magics.Contains(magicId);
        }
    }
}