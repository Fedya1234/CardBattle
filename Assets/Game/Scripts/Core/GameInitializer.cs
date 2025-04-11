using Game.Scripts.Data.Saves;

namespace Game.Scripts.Core
{
    public class GameInitializer
    {
        private OpponentFinder _opponentFinder = new OpponentFinder();

        public async void StartGame()
        {
            var save = GetSave();
            var opponent = await _opponentFinder.FindOpponent(save);
            
            // TODO Start game
            var gameSession = new GameSession(new[] { save, opponent });
        }

        private PlayerSave GetSave()
        {
            return new PlayerSave(); // TODO Get it from Save
        }
    }
}