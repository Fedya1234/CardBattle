using Game.Scripts.Core;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Helpers
{
    /// <summary>
    /// Helper class for UI to access game data
    /// </summary>
    public class GameUIHelper : MonoBehaviour
    {
        private GameInitializer _gameInitializer;
        private GameSession _gameSession;
        
        // Events that UI components can subscribe to
        public delegate void GameStateUpdateHandler(GameState state);
        public event GameStateUpdateHandler OnGameStateUpdated;
        
        public delegate void GameEndHandler(int winnerIndex);
        public event GameEndHandler OnGameEnded;
        
        private void Start()
        {
            // Create the game initializer
            _gameInitializer = new GameInitializer();
            
            // Subscribe to its events
            _gameInitializer.OnGameSessionCreated += HandleGameSessionCreated;
            
            // Start the game
            _gameInitializer.StartGame();
        }
        
        private void HandleGameSessionCreated(GameSession session)
        {
            _gameSession = session;
            
            // Subscribe to session events
            _gameSession.OnGameStateUpdated += HandleGameStateUpdated;
            _gameSession.OnGameEnded += HandleGameEnded;
        }
        
        private void HandleGameStateUpdated(GameState state)
        {
            // Forward the event to UI components
            OnGameStateUpdated?.Invoke(state);
        }
        
        private void HandleGameEnded(int winnerIndex)
        {
            // Forward the event to UI components
            OnGameEnded?.Invoke(winnerIndex);
        }
        
        /// <summary>
        /// Gets the current game state
        /// </summary>
        public GameState GetGameState()
        {
            if (_gameSession == null)
                return null;
                
            return _gameSession.GetGameState();
        }
        
        /// <summary>
        /// Burns a card to gain mana
        /// </summary>
        public void BurnCardForMana(int cardIndex)
        {
            if (_gameSession == null)
                return;
                
            _gameSession.BurnCardForMana(cardIndex);
        }
        
        /// <summary>
        /// Ends the player's turn
        /// </summary>
        public void EndTurn()
        {
            if (_gameSession == null)
                return;
                
            _gameSession.EndTurn();
        }
        
        /// <summary>
        /// Creates a card move (for actual UI implementation)
        /// </summary>
        public CardMove CreateCardMove(CardLevel card, int line, int row)
        {
            GameState state = GetGameState();
            if (state == null)
                return null;
                
            return new CardMove(card, line, row, state.MyIndex);
        }
        
        /// <summary>
        /// Sets the player's move (for actual UI implementation)
        /// </summary>
        public void SetPlayerMove(PlayerMove move)
        {
            if (_gameSession == null)
                return;
                
            // In a real implementation, this would forward the move to the PlayerMoveController
            // For now, it's just a placeholder
            Debug.Log("UI set player move");
        }
    }
}