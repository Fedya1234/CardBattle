using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Core.MoveControllers
{
    public abstract class BaseMoveController
    {
        protected int Index;

        protected BaseMoveController(int index)
        {
            Index = index;
        }
        
        public abstract UniTask<PlayerMoveReplyData> MakeMove();

        public abstract UniTask<ChooseCardsReplyData> ChooseCards(List<CardLevel> cards);
        
    }
}