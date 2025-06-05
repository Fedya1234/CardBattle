using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Saves
{
    [Serializable]
    public class PlayerSave
    {
        public HeroId HeroId;
        public int Level;
        public List<HeroMagicSave> Magics;
        public List<HeroSave> Heroes;
        public List<CardSave> Cards;
    }
}