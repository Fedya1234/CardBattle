using System;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Static
{
    [Serializable]
    public class CardStaticData
    {
        public RarityId Rarity;
        public CardTypeId Type;
        public int ManaCost;
        public CardId Id;
    }
}