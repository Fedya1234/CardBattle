using System;
using Game.Scripts.Save;
using UnityEngine;

namespace Game.Scripts.Data.Saves
{
  [Serializable]
  [CreateAssetMenu(menuName = "SO/Save")]
  public class SaveData : SaveableScriptableObject
  {
    public PlayerSave PlayerSave;
    
    private void OnValidate()
    {
      
    }

    [ContextMenu("Reset Values")]
    public void SetToDefault()
    {
      PlayerSave = new PlayerSave();
    }
  }
}