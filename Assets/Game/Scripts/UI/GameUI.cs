using System.Collections.Generic;
using System.Linq;
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
    
    [Header("Preview")]
    [SerializeField] private GhostUnit _ghostUnitPrefab;
    
    [Header("Controls")]
    [SerializeField] private Button _endTurnButton;
    [SerializeField] private Button _burnCardButton;
    [SerializeField] private GameObject _waitingOverlay;
    
    private GameSession _gameSession;
    private PlayerMoveController _playerController;
    private Dictionary<Vector2Int, BoardUnit> _boardUnits = new Dictionary<Vector2Int, BoardUnit>();
    private Dictionary<Vector2Int, BoardTile> _boardTiles = new Dictionary<Vector2Int, BoardTile>();
    private Dictionary<Vector2Int, GhostUnit> _ghostUnits = new Dictionary<Vector2Int, GhostUnit>();
    private HandCardUI _selectedCardForBurning;
    
    public static GameUI Instance { get; private set; }
    
    private void Awake()
    {
      Instance = this;
    }
    
    /// <summary>
    /// Gets the BoardUnit for a specific player and position
    /// </summary>
    public BoardUnit GetBoardUnit(int playerIndex, int line, int row)
    {
      // For opponent (player 1), offset the row by 3
      var position = playerIndex == 0 ? 
        new Vector2Int(line, row) : 
        new Vector2Int(line, row + 3);
        
      _boardUnits.TryGetValue(position, out var boardUnit);
      return boardUnit;
    }
    
    /// <summary>
    /// Removes a BoardUnit from the dictionary immediately
    /// </summary>
    public void RemoveBoardUnit(BoardUnit boardUnit)
    {
      // Find and remove the BoardUnit from our dictionary
      var toRemove = new List<Vector2Int>();
      foreach (var kvp in _boardUnits)
      {
        if (kvp.Value == boardUnit)
        {
          toRemove.Add(kvp.Key);
        }
      }
      
      foreach (var key in toRemove)
      {
        _boardUnits.Remove(key);
      }
    }
    
    private void Start()
    {
      InitializeBoardTiles();
      SetupUI();
      
      // По умолчанию UI неактивен, пока не запросят ход
      SetUIInteractable(false);
      if (_waitingOverlay != null)
        _waitingOverlay.SetActive(true);
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
      
      // Инициализация _waitingOverlay если его нет
      if (_waitingOverlay == null)
      {
        Debug.LogWarning("GameUI: _waitingOverlay is not assigned. Creating a temporary one.");
        _waitingOverlay = new GameObject("WaitingOverlay");
        _waitingOverlay.transform.SetParent(transform);
        _waitingOverlay.SetActive(false);
      }
    }
    
    private void InitializeBoardTiles()
    {
      // Создаем тайлы для игрового поля игрока (3x3 внизу)
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var position = new Vector2Int(line, row);
          var worldPosition = BoardPositionToWorldPosition(position, false);
          
          var tile = Instantiate(_tilePrefab, worldPosition, Quaternion.identity, _boardContainer);
          tile.Initialize(position);
          tile.name = $"PlayerTile_{line}_{row}";
          _boardTiles[position] = tile;
        }
      }
      
      // Создаем тайлы для поля оппонента (3x3 вверху, позиции 3-5 по row)
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var position = new Vector2Int(line, row + 3); // Смещаем для оппонента
          var worldPosition = BoardPositionToWorldPosition(position, true);
          
          var tile = Instantiate(_tilePrefab, worldPosition, Quaternion.identity, _boardContainer);
          tile.Initialize(position);
          tile.name = $"OpponentTile_{line}_{row}";
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
      if (_waitingOverlay != null)
        _waitingOverlay.SetActive(false);
      
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
        
        // Создаем полупрозрачный предпросмотр юнита
        CreateGhostUnit(card, boardPosition);
        
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
    
    public bool CancelCardPlacement(Vector2Int position)
    {
      if (_playerController == null || !_ghostUnits.TryGetValue(position, out var ghost))
        return false;
      
      // Находим карту в текущем ходе
      var currentMove = _playerController.GetCurrentMove();
      var cardMove = currentMove.Cards.FirstOrDefault(c => c.Line == position.x && c.Row == position.y);
      
      if (cardMove != null)
      {
        // Запоминаем данные карты перед удалением
        var cardData = cardMove.Card;
        
        // Удаляем карту из хода
        bool removed = _playerController.TryRemoveCardFromMove(cardData, position.x, position.y);
        
        if (removed)
        {
          Debug.Log($"GameUI: Cancelled placement of card {cardData.Id} at {position}");
          
          // Удаляем призрак с анимацией
          RemoveGhostUnit(position);
          
          // Обновляем UI
          UpdatePendingMoveDisplay();
          
          // Возвращаем карту в руку
          ReturnCardToHand(cardData);
          
          return true;
        }
      }
      
      return false;
    }
    
    private void CreateGhostUnit(CardLevel card, Vector2Int boardPosition)
    {
      // Удалить предыдущий призрак на этой позиции, если он был
      RemoveGhostUnit(boardPosition);
      
      // Создать новый призрак
      var worldPosition = BoardPositionToWorldPosition(boardPosition, false);
      var ghostUnit = Instantiate(_ghostUnitPrefab, worldPosition, Quaternion.identity, _boardContainer);
      ghostUnit.Setup(card, boardPosition, this);
      
      // Добавить в словарь для отслеживания
      _ghostUnits[boardPosition] = ghostUnit;
    }
    
    private void RemoveGhostUnit(Vector2Int boardPosition)
    {
      if (_ghostUnits.TryGetValue(boardPosition, out var ghost))
      {
        // Удаляем с анимацией
        ghost.PlayRemoveAnimation();
        _ghostUnits.Remove(boardPosition);
      }
    }
    
    private void ClearAllGhostUnits()
    {
      foreach (var position in new List<Vector2Int>(_ghostUnits.Keys))
      {
        RemoveGhostUnit(position);
      }
      
      _ghostUnits.Clear();
    }
    
    private void ReturnCardToHand(CardLevel card)
    {
      // Создать новый UI для карты в руке
      var cardUI = Instantiate(_cardPrefab, _handContainer);
      cardUI.Initialize(card);
      
      // Настраиваем обработчики событий
      cardUI.OnCardSelected += () => SelectCardForBurning(cardUI);
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
      
      // Очищаем все призраки - они больше не нужны, так как ход завершен
      ClearAllGhostUnits();
      
      // Деактивируем UI
      SetUIInteractable(false);
      if (_waitingOverlay != null)
        _waitingOverlay.SetActive(true);
      
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
      // Проверяем, что позиция в пределах игрового поля игрока (0-2 по row)
      if (boardPosition.x < 0 || boardPosition.x >= 3 || 
          boardPosition.y < 0 || boardPosition.y >= 3)
        return false;
        
      // Проверяем, что на позиции нет юнита и нет призрака
      if (_boardUnits.ContainsKey(boardPosition) || _ghostUnits.ContainsKey(boardPosition))
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
      // При обновлении состояния очищаем все призраки
      ClearAllGhostUnits();
      
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
      // Проверка на null для избежания NullReferenceException
      if (gameState == null || gameState.GetMyState() == null || gameState.GetMyState().Board == null)
      {
          Debug.LogWarning("GameUI: Cannot update board units - game state or board is null");
          return;
      }

      // Обновляем юнитов на поле игрока (нижняя половина)
      UpdatePlayerBoard(gameState.GetMyState(), true);
      
      // Обновляем юнитов на поле оппонента (верхняя половина)
      if (gameState.GetOpponentState() != null && gameState.GetOpponentState().Board != null)
      {
          UpdatePlayerBoard(gameState.GetOpponentState(), false);
      }
    }
    
    private void UpdatePlayerBoard(PlayerState playerState, bool isMyBoard)
    {
      for (int line = 0; line < 3; line++)
      {
        for (int row = 0; row < 3; row++)
        {
          var place = playerState.Board.GetPlace(line, row);
          
          // Для поля оппонента смещаем позицию по оси Z (ряды 3-5)
          var position = isMyBoard ? 
            new Vector2Int(line, row) : 
            new Vector2Int(line, row + 3);
          
          if (!place.IsEmpty && !_boardUnits.ContainsKey(position))
          {
            // Создаем нового юнита
            SpawnUnit(place.Unit, position, !isMyBoard);
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
    
    private void SpawnUnit(GameUnitData unitData, Vector2Int boardPosition, bool isOpponent)
    {
      var worldPosition = BoardPositionToWorldPosition(boardPosition, isOpponent);
      var unit = Instantiate(_unitPrefab, worldPosition, Quaternion.identity, _boardContainer);
      unit.Initialize(unitData.UnitState, boardPosition);
      _boardUnits[boardPosition] = unit;
    }
    
    private void DestroyUnit(Vector2Int boardPosition)
    {
      if (_boardUnits.TryGetValue(boardPosition, out var unit))
      {
        _ = unit.PlayDeathAnimation(); // Start death animation asynchronously
        _boardUnits.Remove(boardPosition);
      }
    }
    
    private Vector3 BoardPositionToWorldPosition(Vector2Int boardPosition, bool isOpponent = false)
    {
      // Логика преобразования позиции на поле в мировую позицию
      // Для оппонента смещаем по оси Z на 6 единиц
      float spacing = 2.0f;
      float offsetX = -2.0f; // Центрируем поле 3x3
      float offsetZ = isOpponent ? 4.0f : -2.0f;
      
      return new Vector3(
        offsetX + boardPosition.x * spacing,
        0,
        offsetZ + boardPosition.y * spacing
      );
    }
    
    private void OnDestroy()
    {
      // Clear static instance
      if (Instance == this)
      {
        Instance = null;
      }
      
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
