using UnityEngine;

namespace Game.Scripts.UI.Board
{
  /// <summary>
  /// Компонент для клеток игрового поля, позволяет определить позицию при drag & drop
  /// </summary>
  public class BoardTile : MonoBehaviour
  {
    [SerializeField] private Vector2Int _position;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _highlightMaterial;
        
    public Vector2Int Position => _position;
        
    private bool _isHighlighted;
        
    public void Initialize(Vector2Int position)
    {
      _position = position;
      name = $"BoardTile_{position.x}_{position.y}";
    }
        
    public void SetHighlight(bool highlight)
    {
      if (_isHighlighted == highlight)
        return;
                
      _isHighlighted = highlight;
            
      if (_meshRenderer != null)
      {
        _meshRenderer.material = highlight ? _highlightMaterial : _defaultMaterial;
      }
    }
        
    private void OnMouseEnter()
    {
      // Подсветка при наведении мыши
      SetHighlight(true);
    }
        
    private void OnMouseExit()
    {
      // Убираем подсветку
      SetHighlight(false);
    }
  }
}