using Game.Scripts.Data.Core.State;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Data.Tools
{
  [CreateAssetMenu(fileName = "GameStateView", menuName = "SO/GameStateView", order = 1)]
  public class GameStateView : SerializedScriptableObject
  {
    public GameState GameState;
  }
}