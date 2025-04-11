using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Core
{
    public class BotMoveController : BaseMoveController
    {
        public BotMoveController(int index) : base(index)
        {
        }

        public override async UniTask<PlayerMoveReplyData> MakeMove()
        {
            await UniTask.Delay(1000);
            return new PlayerMoveReplyData(new PlayerMove(), Index);
        }

        public override async UniTask<ChooseCardsReplyData> ChooseCards(List<CardLevel> cards)
        {
            await UniTask.Delay(1000);
            return new ChooseCardsReplyData(cards, new List<CardLevel>(), Index);
        }
    }
}