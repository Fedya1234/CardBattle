using System.Collections.Generic;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.Move
{
    public class ChooseCardsReplyData
    {
        public List<CardLevel> Hand;
        public List<CardLevel> Discard;
        public int Index;
        
        public ChooseCardsReplyData(List<CardLevel> hand, List<CardLevel> discard, int index)
        {
            Hand = hand;
            Discard = discard;
            Index = index;
        }
    }
}