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
                    continue;
                }

                HandCards.Remove(handCard);
                Discard.Add(handCard);

                
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
            return HandCards.FirstOrDefault(handCard => handCard.Equals(card));
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