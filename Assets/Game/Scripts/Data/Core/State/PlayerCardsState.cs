using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Data.Core.State
{
    public class PlayerCardsState
    {
        public List<CardLevel> Deck;
        public List<CardLevel> HandCards;
        public List<CardLevel> Discard;
        

        public PlayerCardsState(PlayerSave save)
        {
            Deck = GetCards(save);
            HandCards = new List<CardLevel>();
            Discard = new List<CardLevel>();
        }

        public PlayerCardsState(List<CardLevel> deck, List<CardLevel> handCards, List<CardLevel> discard)
        {
            Deck = deck;
            HandCards = handCards;
            Discard = discard;
        }

        public List<CardLevel> GetCardsFromDeck(int count)
        {
            var cards = Deck.Take(count).ToList();
            Deck.RemoveRange(0, count);
            return cards;
        }

        public void AddCardsToDeck(List<CardLevel> cards)
        {
            Deck.AddRange(cards);
        }

        public void ApplyChanges(PlayerMove changes)
        {
            foreach (var cardMove in changes.Cards)
            {
                var handCard = HandCard(cardMove.Card);
                if (handCard == null)
                {
                    Debug.LogError($"HandCard not found {cardMove.Card.Id} level = {cardMove.Card.Level}");
                    Debug.LogError($"Available hand cards: {string.Join(", ", HandCards.Select(c => $"{c.Id}:{c.Level}"))}");
                    continue;
                }

                HandCards.Remove(handCard);
                Discard.Add(handCard);
            }
            
            // Apply burned card for mana
            if (changes.BurnedForManaCard != null)
            {
                var burnedCard = HandCard(changes.BurnedForManaCard);
                if (burnedCard != null)
                {
                    HandCards.Remove(burnedCard);
                    Discard.Add(burnedCard);
                    Debug.Log($"Burned card {burnedCard.Id} for mana");
                }
                else
                {
                    Debug.LogError($"Could not find burned card {changes.BurnedForManaCard.Id} level = {changes.BurnedForManaCard.Level} in hand");
                }
            }
        }

        public void TakeCard()
        {
            var nextCard = Deck.FirstOrDefault();
            Deck.Remove(nextCard);
            HandCards.Add(nextCard);
        }

        private CardLevel HandCard(CardLevel card)
        {
            // First try exact match
            var exactMatch = HandCards.FirstOrDefault(handCard => handCard.Equals(card));
            if (exactMatch != null)
                return exactMatch;
                
            // If no exact match, try matching by ID and Level
            return HandCards.FirstOrDefault(handCard => 
                handCard.Id == card.Id && handCard.Level == card.Level);
        }
        
        private List<CardLevel> GetCards(PlayerSave save)
        {
            var cards = new List<CardLevel>();
            foreach (var cardSave in save.Cards)
            {
                for (int i = 0; i < cardSave.Count; i++)
                {
                    cards.Add(new CardLevel(cardSave.Id, cardSave.Level));
                }
            }
            return cards;
        }
    }
}
