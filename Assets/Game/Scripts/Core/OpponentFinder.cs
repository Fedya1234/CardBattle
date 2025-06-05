using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Saves;
using Game.Scripts.Helpers;

namespace Game.Scripts.Core
{
    public class OpponentFinder
    {
        public async UniTask<PlayerSave> FindOpponent(PlayerSave playerSave)
        {
            var score = 0;
            var cards = playerSave.Cards;
            foreach (var cardSave in cards)
            {
                score += cardSave.Count * cardSave.Level;
            }

            await UniTask.Delay(1000); //Wait Immitation

            return FindOpponent(score);
        }

        private PlayerSave FindOpponent(int level)
        {
            return SaveService.GetPlayerSave();// new PlayerSave();//Find Bot Or Opponent
        }
        
    }
}