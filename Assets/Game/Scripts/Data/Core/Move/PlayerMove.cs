using System;
using System.Collections.Generic;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.Move
{
    [Serializable]
    public class PlayerMove
    {
        // Cards being played
        public List<CardMove> Cards = new();
        
        // Card burned for mana (only one allowed per turn as per game rules)
        public CardLevel BurnedForManaCard;
        
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
        
        /// <summary>
        /// Sets a card to be burned for mana
        /// </summary>
        /// <param name="card">The card to burn</param>
        public void BurnCardForMana(CardLevel card)
        {
            BurnedForManaCard = card;
        }
    }
}