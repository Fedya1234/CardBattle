using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Saves
{
    [Serializable]
    public class PlayerSave
    {
        public HeroId HeroId;
        public int Level;
        public List<HeroSave> Heroes;
        public List<CardSave> Cards;
        
        public HeroSave GetHeroSave() => 
            GetHeroSave(HeroId);

        public HeroSave GetHeroSave(HeroId heroId)
        {
            return Heroes.FirstOrDefault(hero => hero.Id == heroId);
        }
    }
}