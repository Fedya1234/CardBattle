using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Data.Core.State;
using TMPro;
using UnityEngine;

namespace Game.Scripts.UI.Board
{
    public class BoardUnit : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private Canvas _statsCanvas; // World Space для отображения HP/DMG
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _damageText;
        
        private UnitState _unitState;
        private Vector2Int _boardPosition;
        
        public void Initialize(UnitState unitState, Vector2Int position)
        {
            _unitState = unitState;
            _boardPosition = position;
            UpdateVisuals();
            PlaySpawnAnimation();
        }
        
        public void UpdateStats(UnitState newUnitState)
        {
            _unitState = newUnitState;
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (_healthText != null)
                _healthText.text = _unitState.Health.ToString();
                
            if (_damageText != null)
                _damageText.text = _unitState.Damage.ToString();
                
            // Обновить модель/текстуры на основе UnitId
            UpdateUnitAppearance();
        }
        
        private void UpdateUnitAppearance()
        {
            // Здесь можно настроить внешний вид юнита на основе его типа
            // Например, поменять материал или модель
            
            // Временная реализация - меняем цвет на основе здоровья
            if (_meshRenderer != null)
            {
                var material = _meshRenderer.material;
                var healthPercent = (float)_unitState.Health / 10f; // Предполагаем макс. здоровье 10
                material.color = Color.Lerp(Color.red, Color.green, healthPercent);
            }
        }
        
        private void PlaySpawnAnimation()
        {
            // Анимация появления
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            
            // Опциональная анимация падения
            var startPos = transform.position + Vector3.up * 3f;
            transform.position = startPos;
            transform.DOMoveY(0f, 0.5f).SetEase(Ease.OutBounce);
        }
        
        public async UniTask PlayAttackAnimation()
        {
            // Анимация атаки с маленьким шейком
            var originalPos = transform.position;
            var sequence = DOTween.Sequence();
            
            // Маленький шейк атакующего юнита
            sequence.Append(transform.DOShakePosition(1f, 0.1f, 10, 90, false, true));
            
            if (_animator != null)
            {
                _animator.SetTrigger("Attack");
            }
            
            // Ждем завершения анимации
            await sequence.AsyncWaitForCompletion();
        }
        
        public async UniTask PlayDeathAnimation()
        {
            // Анимация исчезновения
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
            sequence.Join(transform.DORotate(new Vector3(0, 360, 0), 0.3f, RotateMode.LocalAxisAdd));
            
            // Ждем завершения анимации
            await sequence.AsyncWaitForCompletion();
            
            // Уничтожаем объект после анимации
            Destroy(gameObject);
        }
        
        public void PlayDamageAnimation()
        {
            // Анимация получения урона
            transform.DOShakePosition(0.3f, 0.2f, 10, 90, false, true);
            
            // Временное покраснение
            if (_meshRenderer != null)
            {
                var originalColor = _meshRenderer.material.color;
                _meshRenderer.material.color = Color.red;
                DOTween.To(() => _meshRenderer.material.color, 
                          x => _meshRenderer.material.color = x, 
                          originalColor, 0.3f);
            }
        }
        
        private void OnDestroy()
        {
            // Очищаем DOTween анимации
            transform.DOKill();
        }
    }
}
