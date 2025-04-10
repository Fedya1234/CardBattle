using System;
using System.Collections.Generic;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class PlayerState
    {
        public event Action<PlayerState> EventChanged; 
        private const int LinesCount = 3;
        private const int RowsCount = 3;
        
        public List<CardLevel> HandCards;
        public GameBoardPlace[][] Board;
        public List<CardLevel> Discard;
        public HeroState Hero;

        public PlayerState()
        {
            Board = new GameBoardPlace[LinesCount][];
            for (var i = 0; i < LinesCount; i++)
            {
                Board[i] = new GameBoardPlace[RowsCount];
            }
            HandCards = new List<CardLevel>();
        }
        
        public void AddCardToHand(CardLevel card)
        {
            HandCards.Add(card);
        }
        
        public void RemoveCardFromHand(CardLevel card)
        {
            HandCards.Remove(card);
        }
    }
}