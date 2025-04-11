namespace Game.Scripts.Data.Core.State
{
    public class BoardState
    {
        private const int LinesCount = 3;
        private const int RowsCount = 3;
        
        public GameBoardPlace[][] Board;

        public BoardState()
        {
            Board = new GameBoardPlace[LinesCount][];
            for (var i = 0; i < LinesCount; i++)
            {
                Board[i] = new GameBoardPlace[RowsCount];
            }
        }
        
        public GameBoardPlace GetPlace(int line, int row) => Board[line][row];
        
    }
}