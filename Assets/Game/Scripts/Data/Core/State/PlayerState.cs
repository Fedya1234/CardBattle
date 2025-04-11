using System;
using System.Collections.Generic;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class PlayerState
    {
        public BoardState Board;
        public PlayerCardsState Cards;
        public HeroState Hero;

        public PlayerState(List<CardLevel> deck)
        {
            Board = new BoardState();
            Cards = new PlayerCardsState(deck);
            Hero = new HeroState();
        }

        public void ApplyChanges(PlayerMove changes)
        {
            Cards.ApplyChanges(changes);
        }
    }
}