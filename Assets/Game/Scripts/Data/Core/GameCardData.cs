using System.Collections.Generic;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core
{
    public class GameCardData
    {
        public CardId CardId;
        public int Level;
        public int Health;
        public int Damage;
        public int ManaCost;
        public RarityId Rarity;
        public RaceId Race;
        public List<SkillId> SkillIds;
    }
}