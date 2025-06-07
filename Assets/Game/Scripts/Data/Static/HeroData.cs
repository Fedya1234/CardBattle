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
        
        public bool IsHaveMagic(HeroMagicId magicId)
        {
            return Magics.Contains(magicId);
        }
    }
}