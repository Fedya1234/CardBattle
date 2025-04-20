namespace Game.Scripts.Data.Core.Move
{
    public class PlayerMoveReplyData
    {
        public readonly PlayerMove Move;
        public readonly int Index;
        public readonly EmotionType Emotion;
        
        public PlayerMoveReplyData(PlayerMove move, int index, EmotionType emotion = EmotionType.None)
        {
            Move = move;
            Index = index;
            Emotion = emotion;
        }
    }
    
    /// <summary>
    /// Enum for player emotions that can be shown during gameplay
    /// </summary>
    public enum EmotionType
    {
        None,
        Happy,
        Angry,
        Surprised,
        Sad,
        Confused,
        Confident,
        Nervous
    }
}