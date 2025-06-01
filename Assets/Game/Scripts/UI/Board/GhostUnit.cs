using DG.Tweening;
using TMPro;
using UnityEngine;
using Game.Scripts.Data.Saves;

namespace Game.Scripts.UI.Board
{
    /// <summary>
    /// Полупрозрачный предпросмотр юнита для отображения выбранных позиций размещения карт
    /// </summary>
    public class GhostUnit : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Canvas _statsCanvas;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _damageText;
        
        private Vector2Int _position;
        private CardLevel _cardData;
        private GameUI _gameUI;
        
        public Vector2Int Position => _position;
        public CardLevel CardData => _cardData;
        
        public void Setup(CardLevel cardData, Vector2Int position, GameUI gameUI)
        {
            _cardData = cardData;
            _position = position;
            _gameUI = gameUI;
            
            // Настроить визуальное представление на основе данных карты
            UpdateVisuals();
            
            // Настроить прозрачность
            SetupTransparentMaterials();
            
            // Добавить легкую анимацию появления
            PlaySpawnAnimation();
        }
        
        private void UpdateVisuals()
        {
            // В будущем здесь можно получать реальные данные юнита из статических данных
            // С использованием: StaticDataService.GetUnitData(_cardData.UnitId, _cardData.Level);
            
            if (_healthText != null)
                _healthText.text = "3"; // Временно для демонстрации
            
            if (_damageText != null)
                _damageText.text = "2"; // Временно для демонстрации
        }
        
        private void SetupTransparentMaterials()
        {
            if (_meshRenderer == null || _meshRenderer.materials == null)
                return;
            
            foreach (var material in _meshRenderer.materials)
            {
                // Заменяем на новый материал с поддержкой прозрачности
                Material transparentMaterial = new Material(material);
                
                // Настраиваем прозрачность
                Color color = transparentMaterial.color;
                color.a = 0.6f; // 60% прозрачности
                transparentMaterial.color = color;
                
                // Установка режима прозрачности
                transparentMaterial.SetFloat("_Mode", 3); // Transparent
                transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transparentMaterial.SetInt("_ZWrite", 0);
                transparentMaterial.DisableKeyword("_ALPHATEST_ON");
                transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
                transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentMaterial.renderQueue = 3000;
                
                // Применяем материал
                _meshRenderer.material = transparentMaterial;
            }
            
            // Настраиваем прозрачность для UI элементов
            if (_statsCanvas != null)
            {
                CanvasGroup canvasGroup = _statsCanvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _statsCanvas.gameObject.AddComponent<CanvasGroup>();
                
                canvasGroup.alpha = 0.8f;
            }
        }
        
        private void PlaySpawnAnimation()
        {
            // Простая анимация появления
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
          
            // Если нет LeanTween, можно использовать альтернативный вариант:
            /*
            transform.localScale = Vector3.zero;
            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            */
        }
        
        public void PlayRemoveAnimation()
        {
            // Анимация удаления
            
            transform.localScale = Vector3.one;
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
                
            // Альтернатива для DOTween:
            /*
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
            */
        }
        
        private void OnMouseDown()
        {
            // При клике на призрак, отменить размещение
            if (_gameUI != null)
            {
                _gameUI.CancelCardPlacement(_position);
            }
        }
    }
}