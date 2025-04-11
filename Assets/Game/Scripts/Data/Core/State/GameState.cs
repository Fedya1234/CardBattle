using System;
using System.Collections.Generic;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class GameState
    {
        public PlayerState[] Players;
        public int Round;
        public int MyIndex;
        public int OpponentIndex => MyIndex == 0 ? 1 : 0;

        public PlayerState GetMyState() => GetState(MyIndex);
        public PlayerState GetOpponentState() => GetState(OpponentIndex);
        
        public PlayerState GetState(int index) => Players[index];


        public GameState(PlayerSave[] players, int myTurnIndex)
        {
            MyIndex = myTurnIndex;
            Players = new PlayerState[players.Length];
            
            for (var i = 0; i < players.Length; i++) 
                Players[i] = new PlayerState(players[i]);

            Round = 0;
        }
        
        
    }
}