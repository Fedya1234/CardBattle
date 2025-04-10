using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core
{
    [Serializable]
    public class GameBoardPlace
    {
        public List<PlaceMarkId> PlaceMarks = new List<PlaceMarkId>();
        public GameUnitData Unit;
    }
}