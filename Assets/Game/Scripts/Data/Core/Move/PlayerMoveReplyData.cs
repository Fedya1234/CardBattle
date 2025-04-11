namespace Game.Scripts.Data.Core.Move
{
    public class PlayerMoveReplyData
    {
        public readonly PlayerMove Move;
        public readonly int Index;
        
        public PlayerMoveReplyData(PlayerMove move, int index)
        {
            Move = move;
            Index = index;
        }
    }
}