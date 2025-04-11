using System;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class GameState
    {
        private const int MAX_PLAYERS = 2;
        
        public PlayerState[] Players;
        public int Round;
        public int MyIndex;
        public int OpponentIndex => MyIndex == 0 ? 1 : 0;

        public PlayerState GetMyState() => Players[MyIndex];
        public PlayerState GetOpponentState() => Players[OpponentIndex];


        public GameState()
        {
            Players = new PlayerState[MAX_PLAYERS];
            Round = 0;
        }

        public void Init(PlayerState[] players, int myTurnIndex)
        {
            MyIndex = myTurnIndex;
            Players = players;
        }
        
        
    }
}