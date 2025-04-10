using System;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class CardMove
    {
        public GameUnitData Card { get; private set; }
        public int Line { get; private set; }
        public int Row { get; private set; }
        public int FieldIndex { get; private set; }
        
        public CardMove(GameUnitData card, int line, int row, int fieldIndex)
        {
            Card = card;
            Line = line;
            Row = row;
            FieldIndex = fieldIndex;
        }
    }
}