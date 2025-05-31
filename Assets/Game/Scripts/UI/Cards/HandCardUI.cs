using Game.Scripts.Data.Saves;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;
using Game.Scripts.UI.Board;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.UI.Cards
{
    public class HandCardUI : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _cardImage;
        [SerializeField] private TMP_Text _classText;
        [SerializeField] private TMP_Text _raceText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _manaCostText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private Image _burnedOverlay;
        
        private CardLevel _cardData;
        private Canvas _parentCanvas;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private GameUI _gameUI;
        private bool _isDragging;
        private bool _isSelected;
        private bool _isMarkedForBurning;
        private bool _isInteractable = true;
        
        // Events
        public System.Action OnCardSelected;
        
        public CardLevel CardData => _cardData;
        public bool IsSelected => _isSelected;
        public bool IsMarkedForBurning => _isMarkedForBurning;
        
        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            _gameUI = FindObjectOfType<GameUI>();
            
            // Инициализируем визуальные элементы
            if (_selectionHighlight != null)
                _selectionHighlight.gameObject.SetActive(false);
                
            if (_burnedOverlay != null)
                _burnedOverlay.gameObject.SetActive(false);
        }
        
        public void Initialize(CardLevel cardData)
        {
            _cardData = cardData;
            UpdateVisuals();
        }
        
        public void Initialize(CardLevel cardLevel, CardVisual cardVisual, CardStaticData cardStaticData)
        {
            _cardData = cardLevel;
            
            if (_cardImage != null && cardVisual?.Icon != null)
                _cardImage.sprite = cardVisual.Icon;
                
            if (_classText != null)
                _classText.text = cardVisual?.Class.ToString() ?? "Unknown";
                
            if (_raceText != null)
                _raceText.text = cardVisual?.Race.ToString() ?? "Unknown";
                
            if (_rarityText != null)
                _rarityText.text = cardVisual?.Rarity.ToString() ?? "Common";
                
            if (_nameText != null)
                _nameText.text = cardVisual?.Name ?? $"Card {cardLevel.Id}";
                
            if (_descriptionText != null)
                _descriptionText.text = cardVisual?.Description ?? "No description";
                
            if (_levelText != null)
                _levelText.text = $"{cardLevel.Level}";
                
            if (_manaCostText != null)
                _manaCostText.text = $"{cardStaticData?.ManaCost ?? cardLevel.GetManaCost()}";
            
            // Для unit карт показываем damage/health
            if (_damageText != null && _healthText != null)
            {
                // Здесь нужно получить данные юнита из статических данных
                _damageText.text = "2"; // Временно
                _healthText.text = "3"; // Временно
            }
        }
        
        private void UpdateVisuals()
        {
            if (_nameText != null)
                _nameText.text = $"Card {_cardData.Id}";
                
            if (_manaCostText != null)
                _manaCostText.text = _cardData.GetManaCost().ToString();
                
            // Для unit карт показываем damage/health
            if (_damageText != null && _healthText != null)
            {
                // Здесь нужно получить данные юнита из статических данных
                _damageText.text = "2"; // Временно
                _healthText.text = "3"; // Временно
            }
            
            // Обновить изображение карты из VisualService
            UpdateCardImage();
        }
        
        private void UpdateCardImage()
        {
            // Здесь можно получить спрайт карты через VisualService
            // var cardVisual = VisualService.GetCardVisual(_cardData.Id);
            // if (cardVisual != null && _cardImage != null)
            //     _cardImage.sprite = cardVisual.CardSprite;
        }
        
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            
            if (_selectionHighlight != null)
                _selectionHighlight.gameObject.SetActive(selected);
        }
        
        public void SetMarkedForBurning(bool marked)
        {
            _isMarkedForBurning = marked;
            
            if (_burnedOverlay != null)
                _burnedOverlay.gameObject.SetActive(marked);
                
            // Делаем карту менее прозрачной если она помечена для сжигания
            if (_canvasGroup != null)
                _canvasGroup.alpha = marked ? 0.5f : 1f;
        }
        
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
            
            if (_canvasGroup != null)
                _canvasGroup.interactable = interactable;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isInteractable || _isDragging)
                return;
                
            // Клик по карте - выбираем для сжигания или показываем детали
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnCardSelected?.Invoke();
                Debug.Log($"Selected card: {_cardData.Id}");
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Правый клик - показать детали карты
                ShowCardDetails();
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isInteractable || _isMarkedForBurning)
                return;
                
            _isDragging = true;
            //_originalPosition = transform.position;
            _originalParent = transform.parent;
            
            transform.SetParent(_parentCanvas.transform, true);
            // Делаем карту полупрозрачной
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0.6f;
                
            // Поднимаем карту выше других элементов
            transform.SetAsLastSibling();
            
            Debug.Log($"Started dragging card: {_cardData.Id}");
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
                
            if (_parentCanvas != null)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentCanvas.transform as RectTransform,
                    eventData.position,
                    _parentCanvas.worldCamera,
                    out position);
                    
                transform.position = _parentCanvas.transform.TransformPoint(position);
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
                
            _isDragging = false;
            
            // Возвращаем прозрачность
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
                
            // Проверяем, попали ли на игровое поле
            var boardPosition = GetBoardPositionFromScreenPoint(eventData.position);
            if (boardPosition.HasValue)
            {
                // Пытаемся разместить карту
                if (_gameUI != null && _gameUI.TryPlaceCard(_cardData, boardPosition.Value))
                {
                    // Карта успешно размещена - удаляем из руки
                    Debug.Log($"Card {_cardData.Id} successfully placed at {boardPosition.Value}");
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log($"Failed to place card {_cardData.Id} at {boardPosition.Value}");
                }
            }
            
            // Возвращаем карту в исходную позицию
            //transform.position = _originalPosition;
            transform.SetParent(_originalParent);
        }
        
        private Vector2Int? GetBoardPositionFromScreenPoint(Vector2 screenPoint)
        {
            if (_gameUI != null)
            {
                return _gameUI.GetBoardPosition(screenPoint);
            }
            
            return null;
        }
        
        private void ShowCardDetails()
        {
            // Здесь можно показать подробную информацию о карте
            Debug.Log($"Showing details for card: {_cardData.Id}, Level: {_cardData.Level}, Mana: {_cardData.GetManaCost()}");
        }
    }
}
