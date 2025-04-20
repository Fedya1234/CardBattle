using UnityEngine;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Visual
{
    /// <summary>
    /// Visual representation data for a unit
    /// </summary>
    [System.Serializable]
    public class UnitVisual
    {
        public string Name;
        public string Description;
        public Sprite Icon;
        public ClassId Class;
        public RaceId Race;
        public RarityId Rarity;
    }
}