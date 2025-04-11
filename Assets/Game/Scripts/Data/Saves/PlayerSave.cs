using System;
using System.Collections.Generic;

namespace Game.Scripts.Data.Saves
{
    [Serializable]
    public class PlayerSave
    {
        public int Health;
        public int Mana;
        public int Level;
        public List<CardSave> Cards;
    }
}