using System;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using Game.Scripts.Helpers;

namespace Game.Scripts.Data.Saves
{
    [Serializable]
    public class CardLevel
    {
        public CardId Id;
        public int Level;
        
        public bool Equals(CardLevel other)
        {
            return Id == other.Id && Level == other.Level;
        }
        
        public CardLevel(CardId id, int level)
        {
            Id = id;
            Level = level;
        }
        
        /// <summary>
        /// Gets the mana cost for this card
        /// </summary>
        /// <returns>The card's mana cost based on its ID</returns>
        public int GetManaCost()
        {
            return GetCardData().ManaCost;
        }
        
        /// <summary>
        /// Gets the full static data for this card
        /// </summary>
        /// <returns>The card's static data</returns>
        public CardStaticData GetCardData()
        {
            return StaticDataService.GetCardData(Id);
        }
    }
}