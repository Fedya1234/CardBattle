using System;
using System.Collections.Generic;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class PlayerMove
    {
        public List<CardMove> Cards { get; set; }
        public int ManaDelta { get; set; }
        public int HealthDelta { get; set; }
    }
}