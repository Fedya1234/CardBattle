using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Core.MoveControllers
{
    public class TestMoveController : BaseMoveController
    
    {
        public TestMoveController(int index) : base(index)
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
            var hand = cards
                .OrderBy(_=> Random.Range(0, 100))
                .Take(Random.Range(0, cards.Count)).ToList();
            
            var discard = cards.Except(hand).ToList();
            
            return new ChooseCardsReplyData(hand, discard, Index);
        }
        
    }
}