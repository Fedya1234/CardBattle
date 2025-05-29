using System.Collections.Generic;
using Game.Scripts.Core;
using Game.Scripts.Core.MoveControllers;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Saves;
using Game.Scripts.UI.Board;
using Game.Scripts.UI.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
  public class GameUI : MonoBehaviour
  {
    [Header("Hand UI")]
    [SerializeField] private Transform _handContainer;
    [SerializeField] private HandCardUI _cardPrefab;
    
    [Header("Board")]
    [SerializeField] private Transform _boardContainer;
    [SerializeField] private BoardUnit _unitPrefab;
    [SerializeField] private BoardTile _tilePrefab;
    [SerializeField] private Camera _gameCamera;
    
    [Header("Controls")]
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private Button _burnCardButton;
    
    private GameSession _gameSession;
    private PlayerMoveController _playerController;
    private Dictionary<Vector2Int, BoardUnit> _boardUnits = new Dictionary<Vector2Int, BoardUnit>();
    private Dictionary<Vector2Int, BoardTile> _boardTiles = new Dictionary<Vector2Int, BoardTile>();
    private HandCardUI _selectedCardForBurning;
    
    private void Start()
    {
      InitializeBoardTiles();
      SetupUI();
    }
    
    private void SetupUI()
    {
      if (_endTurnButton != null)
      {
        _endTurnButton.onClick.AddListener(OnEndTurnClicked);
      }
      
      if (_burnCardButton != null)
      {
        _burnCardButton.onClick.AddListener(OnBurnCardClicked);
        _burnCardButton.interactable = false; // Изначально недоступна
      }
    }
    
    private void InitializeBoardTiles()
    {
      // Создаем тайлы для игрового поля 3x3
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var position = new Vector2Int(line, row);
          var worldPosition = BoardPositionToWorldPosition(position);
          
          var tile = Instantiate(_tilePrefab, worldPosition, Quaternion.identity, _boardContainer);
          tile.Initialize(position);
          _boardTiles[position] = tile;
        }
      }
    }
    
    public void SetGameSession(GameSession gameSession)
    {
      Debug.Log("GameUI: Setting game session and connecting to player controller");
      
      _gameSession = gameSession;
      _gameSession.OnGameStateUpdated += OnGameStateUpdated;
      
      // Получаем настоящий PlayerMoveController из GameSession
      _playerController = _gameSession.GetPlayerMoveController();
      
      if (_playerController != null)
      {
        // Подписываемся на события контроллера
        _playerController.OnRequestMove += OnPlayerMoveRequested;
        _playerController.OnRequestCardChoices += OnCardChoicesRequested;
        Debug.Log("GameUI: Successfully connected to PlayerMoveController");
      }
      else
      {
        Debug.LogError("GameUI: Failed to get PlayerMoveController from GameSession!");
      }
    }
    
    private void OnPlayerMoveRequested()
    {
      Debug.Log("GameUI: Player move requested - UI is now active");
      
      // Активируем интерфейс для хода игрока
      SetUIInteractable(true);
      
      // Показываем подсказку игроку
      ShowPlayerHint("Your turn! Play cards or click End Turn");
    }
    
    private void OnCardChoicesRequested(List<CardLevel> cards)
    {
      Debug.Log($"GameUI: Card choice requested for {cards.Count} cards");
      
      // Здесь можно показать специальный UI для выбора карт (мулиган)
      // Пока что автоматически оставляем все карты
      _playerController.SetCardChoices(cards, new List<CardLevel>());
    }
    
    public bool TryPlaceCard(CardLevel card, Vector2Int boardPosition)
    {
      if (_gameSession == null || _playerController == null)
      {
        Debug.LogWarning("GameUI: Cannot place card - no game session or player controller");
        return false;
      }
        
      // Проверяем, можно ли разместить карту
      if (!CanPlaceCard(card, boardPosition))
      {
        Debug.LogWarning($"GameUI: Cannot place card {card.Id} at {boardPosition} - validation failed");
        return false;
      }
        
      // Пытаемся добавить карту к ходу через PlayerMoveController
      bool success = _playerController.TryAddCardToMove(card, boardPosition.x, boardPosition.y);
      
      if (success)
      {
        Debug.Log($"GameUI: Successfully queued card {card.Id} for placement at {boardPosition}");
        
        // Обновляем UI для показа запланированного хода
        UpdatePendingMoveDisplay();
        return true;
      }
      else
      {
        Debug.LogWarning($"GameUI: Failed to queue card {card.Id} for placement");
      }
      
      return false;
    }
    
    public void SelectCardForBurning(HandCardUI cardUI)
    {
      // Отменяем предыдущий выбор
      if (_selectedCardForBurning != null)
      {
        _selectedCardForBurning.SetSelected(false);
      }
      
      _selectedCardForBurning = cardUI;
      
      if (cardUI != null)
      {
        cardUI.SetSelected(true);
        _burnCardButton.interactable = true;
      }
      else
      {
        _burnCardButton.interactable = false;
      }
    }
    
    private void OnBurnCardClicked()
    {
      if (_selectedCardForBurning == null || _playerController == null)
        return;
        
      bool success = _playerController.TryBurnCardForMana(_selectedCardForBurning.CardData);
      
      if (success)
      {
        Debug.Log($"GameUI: Card {_selectedCardForBurning.CardData.Id} marked for burning");
        
        // Визуально отмечаем карту как "сожженную"
        _selectedCardForBurning.SetMarkedForBurning(true);
        
        // Сбрасываем выбор
        SelectCardForBurning(null);
        
        UpdatePendingMoveDisplay();
      }
    }
    
    private void OnEndTurnClicked()
    {
      if (_playerController == null)
      {
        Debug.LogWarning("GameUI: Cannot end turn - no player controller");
        return;
      }
        
      Debug.Log("GameUI: Player ending turn");
      
      // Завершаем ход игрока
      _playerController.FinishMove();
      
      // Деактивируем UI
      SetUIInteractable(false);
      
      ShowPlayerHint("Waiting for opponent...");
    }
    
    private void SetUIInteractable(bool interactable)
    {
      if (_endTurnButton != null)
        _endTurnButton.interactable = interactable;
        
      if (_burnCardButton != null)
        _burnCardButton.interactable = interactable && _selectedCardForBurning != null;
        
      // Можно также активировать/деактивировать drag & drop на картах
      foreach (Transform child in _handContainer)
      {
        var cardUI = child.GetComponent<HandCardUI>();
        if (cardUI != null)
        {
          cardUI.SetInteractable(interactable);
        }
      }
    }
    
    private void UpdatePendingMoveDisplay()
    {
      if (_playerController == null)
        return;
        
      var currentMove = _playerController.GetCurrentMove();
      
      // Обновляем подсветку тайлов для показа запланированных ходов
      foreach (var tile in _boardTiles.Values)
      {
        tile.SetHighlight(false);
      }
      
      foreach (var cardMove in currentMove.Cards)
      {
        var position = new Vector2Int(cardMove.Line, cardMove.Row);
        if (_boardTiles.TryGetValue(position, out var tile))
        {
          tile.SetHighlight(true);
        }
      }
      
      // Обновляем текст кнопки End Turn
      if (_endTurnButton != null)
      {
        var buttonText = _endTurnButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (buttonText != null)
        {
          int cardCount = currentMove.Cards.Count;
          string burnInfo = currentMove.BurnedForManaCard != null ? " + Burn" : "";
          buttonText.text = cardCount > 0 ? $"End Turn ({cardCount} cards{burnInfo})" : "End Turn";
        }
      }
    }
    
    private void ShowPlayerHint(string message)
    {
      // Здесь можно показать подсказку игроку в UI
      Debug.Log($"GameUI Player Hint: {message}");
    }
    
    public Vector2Int? GetBoardPosition(Vector2 screenPosition)
    {
      // Кастуем луч от камеры через экранную точку
      var ray = _gameCamera.ScreenPointToRay(screenPosition);
      
      if (Physics.Raycast(ray, out RaycastHit hit))
      {
        // Проверяем, попали ли в тайл игрового поля
        var boardTile = hit.collider.GetComponent<BoardTile>();
        if (boardTile != null)
        {
          return boardTile.Position;
        }
      }
      
      return null;
    }
    
    private bool CanPlaceCard(CardLevel card, Vector2Int boardPosition)
    {
      // Проверяем, что позиция в пределах поля
      if (boardPosition.x < 0 || boardPosition.x >= 3 || 
          boardPosition.y < 0 || boardPosition.y >= 3)
        return false;
        
      // Проверяем, что на позиции нет юнита
      if (_boardUnits.ContainsKey(boardPosition))
        return false;
        
      // Проверяем, что у игрока достаточно маны
      if (_gameSession != null)
      {
        var playerState = _gameSession.GetGameState().GetMyState();
        if (playerState.Hero.Mana < card.GetManaCost())
          return false;
      }
      
      return true;
    }
    
    private void OnGameStateUpdated(GameState gameState)
    {
      var playerCards = gameState.GetMyState().Cards.HandCards;
      UpdateHandDisplay(playerCards);
      UpdateBoardUnits(gameState);
    }
    
    private void UpdateHandDisplay(List<CardLevel> handCards)
    {
      // Очищаем текущие карты в руке
      foreach (Transform child in _handContainer)
      {
        Destroy(child.gameObject);
      }
      
      // Создаем новые UI карты
      foreach (var card in handCards)
      {
        var cardUI = Instantiate(_cardPrefab, _handContainer);
        cardUI.Initialize(card);
        
        // Настраиваем обработчики событий
        cardUI.OnCardSelected += () => SelectCardForBurning(cardUI);
      }
    }
    
    private void UpdateBoardUnits(GameState gameState)
    {
      // Обновляем юнитов на поле на основе BoardState
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var place = gameState.GetMyState().Board.GetPlace(line, row);
          var position = new Vector2Int(line, row);
          
          if (!place.IsEmpty && !_boardUnits.ContainsKey(position))
          {
            // Создаем нового юнита
            SpawnUnit(place.Unit, position);
          }
          else if (place.IsEmpty && _boardUnits.ContainsKey(position))
          {
            // Удаляем юнита
            DestroyUnit(position);
          }
          else if (!place.IsEmpty && _boardUnits.ContainsKey(position))
          {
            // Обновляем существующего юнита
            _boardUnits[position].UpdateStats(place.Unit.UnitState);
          }
        }
      }
    }
    
    private void SpawnUnit(GameUnitData unitData, Vector2Int boardPosition)
    {
      var worldPosition = BoardPositionToWorldPosition(boardPosition);
      var unit = Instantiate(_unitPrefab, worldPosition, Quaternion.identity, _boardContainer);
      unit.Initialize(unitData.UnitState, boardPosition);
      _boardUnits[boardPosition] = unit;
    }
    
    private void DestroyUnit(Vector2Int boardPosition)
    {
      if (_boardUnits.TryGetValue(boardPosition, out var unit))
      {
        unit.PlayDeathAnimation(); // Анимация будет сама уничтожать объект
        _boardUnits.Remove(boardPosition);
      }
    }
    
    private Vector3 BoardPositionToWorldPosition(Vector2Int boardPosition)
    {
      // Логика преобразования позиции на поле в мировую позицию
      // Например: каждая клетка 2x2 юнита, начинаем с (-2, 0, -2)
      float spacing = 2.0f;
      float offsetX = -2.0f; // Центрируем поле 3x3
      float offsetZ = -2.0f;
      
      return new Vector3(
        offsetX + boardPosition.x * spacing,
        0,
        offsetZ + boardPosition.y * spacing
      );
    }
    
    private void OnDestroy()
    {
      // Отписываемся от событий
      if (_playerController != null)
      {
        _playerController.OnRequestMove -= OnPlayerMoveRequested;
        _playerController.OnRequestCardChoices -= OnCardChoicesRequested;
      }
      
      if (_gameSession != null)
      {
        _gameSession.OnGameStateUpdated -= OnGameStateUpdated;
      }
    }
  }
}
