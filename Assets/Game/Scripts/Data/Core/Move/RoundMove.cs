using System;
using System.Collections.Generic;

namespace Game.Scripts.Data.Core.Move
{
    [Serializable]
    public class RoundMove
    {
        public List<PlayerMove> Moves = new ();
        
        public void AddMove(PlayerMove move)
        {
            Moves.Add(move);
        }
        
        public void Clear()
        {
            Moves.Clear();
        }
    }
}