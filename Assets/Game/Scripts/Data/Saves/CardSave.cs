using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Saves
{
    public class CardSave : CardLevel
    {
        public int Count;

        public CardSave(CardId id, int level, int count) : base(id, level)
        {
            Count = count;
        }
    }
}