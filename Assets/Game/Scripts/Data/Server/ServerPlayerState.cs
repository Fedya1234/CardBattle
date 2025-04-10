using System;
using System.Collections.Generic;

namespace Game.Scripts.Data.Server
{
    [Serializable]
    public class ServerPlayerState
    {
        public List<ServerCard> Deck { get; private set; }
        public List<ServerCard> Hand { get; private set; }
        public List<ServerCard> Discard { get; private set; }
        // List of CardMove here for discarded cards
    }
}