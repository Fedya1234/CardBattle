using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Helpers
{
    /// <summary>
    /// Static utility class for generating card collections
    /// </summary>
    public static class CardGenerator
    {
        /// <summary>
        /// Generates a starter deck for a new player
        /// </summary>
        public static List<CardSave> GenerateStarterDeck()
        {
            var deck = new List<CardSave>
            {
                // Basic unit cards
                new CardSave(CardId.Card_0, 1, 8),  // 8 basic units
                
                // Basic damage spell
                new CardSave(CardId.Card_1, 1, 4),  // 4 damage spells
                
                // Basic buff spell
                new CardSave(CardId.Card_2, 1, 3)   // 3 buff spells
            };
            
            return deck;
        }
        
        /// <summary>
        /// Generates a random deck based on player level
        /// </summary>
        public static List<CardSave> GenerateRandomDeck(int playerLevel)
        {
            var deck = new List<CardSave>();
            int totalCards = 0;
            int maxCards = 30; // Maximum deck size
            
            // Add unit cards
            int unitCards = Mathf.Min(15, 5 + playerLevel);
            AddRandomCards(deck, CardId.Card_0, 1, unitCards);
            totalCards += unitCards;
            
            // Add damage spell cards
            int damageCards = Mathf.Min(10, 3 + playerLevel / 2);
            AddRandomCards(deck, CardId.Card_1, 1, damageCards);
            totalCards += damageCards;
            
            // Add buff spell cards
            int buffCards = Mathf.Min(maxCards - totalCards, 2 + playerLevel / 3);
            AddRandomCards(deck, CardId.Card_2, 1, buffCards);
            
            return deck;
        }
        
        /// <summary>
        /// Helper method to add random cards to a deck
        /// </summary>
        private static void AddRandomCards(List<CardSave> deck, CardId cardId, int level, int count)
        {
            deck.Add(new CardSave(cardId, level, count));
        }
    }
}