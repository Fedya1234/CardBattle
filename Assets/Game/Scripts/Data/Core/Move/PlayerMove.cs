using System;
using System.Collections.Generic;

namespace Game.Scripts.Data.Core.Move
{
    [Serializable]
    public class PlayerMove
    {
        public List<CardMove> Cards = new ();
        
        public void AddCard(CardMove cardMove)
        {
            Cards.Add(cardMove);
        }
        
        public void RemoveLastCard()
        {
            if (Cards.Count == 0)
                return;
            
            Cards.RemoveAt(Cards.Count - 1);
        }
    }
}