using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Tools;
using UnityEngine;
using Game.Scripts.UI;

namespace Game.Scripts.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameUI _gameUI;
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private GameStateView _gameStateView;
        
        private GameInitializer _gameInitializer;
        
        private void Start()
        {
            ShowLoadingScreen(true);
            InitializeGame().Forget();
        }
        
        private async UniTaskVoid InitializeGame()
        {
            Debug.Log("Game Bootstrap: Starting game initialization...");
            
            // Создаем GameInitializer
            _gameInitializer = new GameInitializer();
            
            // Подписываемся на создание GameSession
            _gameInitializer.OnGameSessionCreated += OnGameSessionCreated;
            
            // Запускаем игру
            await _gameInitializer.StartGame();
        }
        
        private void OnGameSessionCreated(GameSession gameSession)
        {
            ShowLoadingScreen(false);
            Debug.Log("Game Bootstrap: GameSession created, connecting to UI...");
            _gameStateView.GameState = gameSession.GetGameState();
            // Если UI найден, связываем его с игровой сессией
            if (_gameUI == null)
            {
                _gameUI = FindObjectOfType<GameUI>();
            }
            
            if (_gameUI != null)
            {
                _gameUI.SetGameSession(gameSession);
                Debug.Log("Game Bootstrap: GameUI successfully connected to GameSession");
            }
            else
            {
                Debug.LogError("Game Bootstrap: GameUI not found! Game will run without UI.");
            }
        }
        
        private void ShowLoadingScreen(bool show)
        {
            if (_loadingScreen != null)
            {
                _loadingScreen.SetActive(show);
            }
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_gameInitializer != null)
            {
                _gameInitializer.OnGameSessionCreated -= OnGameSessionCreated;
            }
        }
    }
}
