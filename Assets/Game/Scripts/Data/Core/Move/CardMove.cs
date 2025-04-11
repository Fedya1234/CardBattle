using System;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.Move
{
    [Serializable]
    public class CardMove
    {
        public CardLevel Card { get; private set; }
        public int Line { get; private set; }
        public int Row { get; private set; }
        public int FieldIndex { get; private set; }
        
        public CardMove(CardLevel card, int line, int row, int fieldIndex)
        {
            Card = card;
            Line = line;
            Row = row;
            FieldIndex = fieldIndex;
        }
    }
}