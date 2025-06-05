using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using UnityEngine;

namespace Game.Scripts.Core.MoveControllers
{
    public class PlayerMoveController : BaseMoveController
    {
        private PlayerMove _pendingMove;
        private List<CardLevel> _pendingCardChoices;
        private List<CardLevel> _discardedCards;
        private bool _moveReady;
        private bool _cardChoicesReady;
        private bool _waitingForInput;
        private GameState _gameState; // Добавляем ссылку на состояние игры для проверки маны
        
        // Events for UI communication
        public delegate void RequestMoveHandler();
        public delegate void RequestCardChoicesHandler(List<CardLevel> cards);
        
        public event RequestMoveHandler OnRequestMove;
        public event RequestCardChoicesHandler OnRequestCardChoices;
        
        // We'll simulate player input being "ready" after a delay for testing purposes
        private const int SimulatedDecisionDelay = 1000; // milliseconds
        
        public PlayerMoveController(int index) : base(index)
        {
            ResetState();
        }
        
        /// <summary>
        /// Устанавливает текущее состояние игры для проверки маны
        /// </summary>
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }
        
        private void ResetState()
        {
            _pendingMove = new PlayerMove();
            _pendingCardChoices = new List<CardLevel>();
            _discardedCards = new List<CardLevel>();
            _moveReady = false;
            _cardChoicesReady = false;
            _waitingForInput = false;
        }
        
        public override async UniTask<PlayerMoveReplyData> MakeMove()
        {
            ResetState();
            _waitingForInput = true;
            
            // Уведомляем UI что нужно ввести ход
            OnRequestMove?.Invoke();
            
            // Ждем пока UI не установит ход
            while (!_moveReady && _waitingForInput)
            {
                await UniTask.Delay(50); // Проверяем каждые 50ms
            }
            
            _waitingForInput = false;
            return new PlayerMoveReplyData(_pendingMove, Index);
        }

        public override async UniTask<ChooseCardsReplyData> ChooseCards(List<CardLevel> cards)
        {
            ResetState();
            _waitingForInput = true;
            
            // Уведомляем UI что нужно выбрать карты
            OnRequestCardChoices?.Invoke(cards);
            
            // Ждем пока UI не установит выбор карт
            while (!_cardChoicesReady && _waitingForInput)
            {
                await UniTask.Delay(50); // Проверяем каждые 50ms
            }
            
            _waitingForInput = false;
            return new ChooseCardsReplyData(_pendingCardChoices, _discardedCards, Index);
        }
        
        /// <summary>
        /// Добавляет карту к текущему ходу (вызывается из UI при размещении карты)
        /// </summary>
        public bool TryAddCardToMove(CardLevel card, int line, int row)
        {
            if (!_waitingForInput)
                return false;
                
            // Проверяем валидность хода
            if (line < 0 || line >= 3 || row < 0 || row >= 3)
                return false;
                
            // Проверяем, что карта еще не была сыграна в этом ходу
            if (_pendingMove.Cards.Any(c => c.Card.Id == card.Id && c.Line == line && c.Row == row))
                return false;
                
            // Проверяем достаточно ли маны для игры этой карты
            if (_gameState != null)
            {
                var playerState = _gameState.GetState(Index);
                int currentMana = playerState.Hero.Mana;
                int totalManaNeeded = CalculateTotalManaNeeded(card);
                
                if (currentMana < totalManaNeeded)
                {
                    Debug.LogWarning($"Not enough mana to play card {card.Id}. Current mana: {currentMana}, needed: {totalManaNeeded}");
                    return false;
                }
            }
            
            var cardMove = new CardMove(card, line, row, Index);
            _pendingMove.AddCard(cardMove);
            
            Debug.Log($"Added card {card.Id} to move at position ({line}, {row})");
            return true;
        }
        
        /// <summary>
        /// Вычисляет общую ману, необходимую для текущего хода включая новую карту
        /// </summary>
        private int CalculateTotalManaNeeded(CardLevel newCard)
        {
            int totalMana = 0;
            
            // Добавляем стоимость всех карт уже в ходу
            foreach (var cardMove in _pendingMove.Cards)
            {
                totalMana += cardMove.Card.GetManaCost();
            }
            
            // Добавляем стоимость новой карты
            totalMana += newCard.GetManaCost();
            
            return totalMana;
        }
        
        /// <summary>
        /// Проверяет можно ли сыграть карту с текущим количеством маны
        /// </summary>
        public bool CanPlayCard(CardLevel card)
        {
            if (_gameState == null)
                return true; // Если нет состояния игры, разрешаем (для тестов)
                
            var playerState = _gameState.GetState(Index);
            int currentMana = playerState.Hero.Mana;
            int totalManaNeeded = CalculateTotalManaNeeded(card);
            
            return currentMana >= totalManaNeeded;
        }
        
        /// <summary>
        /// Возвращает текущее доступное количество маны
        /// </summary>
        public int GetAvailableMana()
        {
            if (_gameState == null)
                return 0;
                
            var playerState = _gameState.GetState(Index);
            int currentMana = playerState.Hero.Mana;
            int usedMana = 0;
            
            // Вычитаем ману уже потраченную в текущем ходу
            foreach (var cardMove in _pendingMove.Cards)
            {
                usedMana += cardMove.Card.GetManaCost();
            }
            
            return currentMana - usedMana;
        }
        
        /// <summary>
        /// Удаляет карту из текущего хода (если игрок передумал)
        /// </summary>
        public bool TryRemoveCardFromMove(CardLevel card, int line, int row)
        {
            if (!_waitingForInput)
                return false;
                
            var cardToRemove = _pendingMove.Cards.FirstOrDefault(c => 
                c.Card.Id == card.Id && c.Line == line && c.Row == row);
                
            if (cardToRemove != null)
            {
                _pendingMove.Cards.Remove(cardToRemove);
                Debug.Log($"Removed card {card.Id} from move at position ({line}, {row})");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Добавляет карту для сжигания за ману
        /// </summary>
        public bool TryBurnCardForMana(CardLevel card)
        {
            if (!_waitingForInput)
                return false;
                
            if (_pendingMove.BurnedForManaCard != null)
            {
                Debug.LogWarning("Already have a card set for burning this turn");
                return false;
            }
            
            _pendingMove.BurnedForManaCard = card;
            Debug.Log($"Set card {card.Id} to burn for mana");
            return true;
        }
        
        /// <summary>
        /// Убирает карту из списка для сжигания
        /// </summary>
        public void ClearBurnedCard()
        {
            if (_waitingForInput)
            {
                _pendingMove.BurnedForManaCard = null;
            }
        }
        
        /// <summary>
        /// Завершает ход игрока (вызывается из UI кнопкой "End Turn")
        /// </summary>
        public void FinishMove()
        {
            if (!_waitingForInput)
                return;
                
            _moveReady = true;
            Debug.Log($"Player {Index} finished move with {_pendingMove.Cards.Count} cards");
        }
        
        /// <summary>
        /// Устанавливает выбор карт в мулигане (вызывается из UI)
        /// </summary>
        public void SetCardChoices(List<CardLevel> chosenCards, List<CardLevel> discardedCards)
        {
            if (!_waitingForInput)
                return;
                
            _pendingCardChoices = new List<CardLevel>(chosenCards);
            _discardedCards = new List<CardLevel>(discardedCards);
            _cardChoicesReady = true;
            
            Debug.Log($"Player {Index} chose {chosenCards.Count} cards, discarded {discardedCards.Count}");
        }
        
        /// <summary>
        /// Отменяет текущий ввод (например, при выходе из игры)
        /// </summary>
        public void CancelInput()
        {
            _waitingForInput = false;
            _moveReady = true; // Отправляем пустой ход
            _cardChoicesReady = true;
        }
        
        /// <summary>
        /// Возвращает текущий статус ожидания ввода
        /// </summary>
        public bool IsWaitingForInput => _waitingForInput;
        
        /// <summary>
        /// Возвращает текущий ход для предварительного просмотра в UI
        /// </summary>
        public PlayerMove GetCurrentMove() => _pendingMove;
        
        /// <summary>
        /// Генерирует случайный ход для тестирования (можно использовать для AI режима)
        /// </summary>
        private PlayerMove GenerateRandomMove()
        {
            var move = new PlayerMove();
            
            // Simulating playing 1-2 cards
            int cardsToPlay = Random.Range(1, 3);
            
            for (int i = 0; i < cardsToPlay; i++)
            {
                var cardMove = new CardMove(
                    new CardLevel(
                        (CardId)Random.Range(0, 3),  // Random card ID (0-2)
                        1),                         // Level 1
                    Random.Range(0, 3),              // Random line (0-2)
                    Random.Range(0, 3),              // Random row (0-2)
                    Index                            // Player index
                );
                
                move.AddCard(cardMove);
            }
            
            return move;
        }
    }
}
