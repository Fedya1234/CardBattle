using System;
using UnityEngine.Serialization;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class GameState
    {
        private const int MAX_PLAYERS = 2;
        
        public PlayerState[] Players;
        public int Round;
        public int PlayersTurn;
        public int MyIndex;

        public bool IsMyTurn => PlayersTurn == MyIndex;
        public bool IsOpponentTurn => PlayersTurn != MyIndex;


        public PlayerState GetMyState() => Players[MyIndex];
        public PlayerState GetOpponentState() => Players[MyIndex == 0 ? 1 : 0];


        public GameState()
        {
            Players = new PlayerState[MAX_PLAYERS];
            Round = 0;
            PlayersTurn = 0;
        }

        public void Init(PlayerState[] players, int myTurnIndex)
        {
            MyIndex = myTurnIndex;
            Players = players;
        }
        
        
    }
}