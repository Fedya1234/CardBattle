using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Save
{
  public class SaveableScriptableObject : SerializedScriptableObject
  {
    private bool _isLoaded;
    
    private void OnEnable()
    {
      Load();
      //Debug.Log("Save Data Loaded");
    }
    public void Load()
    {
      if (_isLoaded)
        return;
      
      string loadedString = PlayerPrefs.GetString(name, SaveString());
      Load(loadedString);
      //Debug.Log("LOAD:" + loadedString);
    }

    public void Save()
    {
      PlayerPrefs.SetString(name, SaveString());
      PlayerPrefs.Save();
//      Debug.Log("SAVE:" + text);
    }
    
    public void Load(string loadedString)
    {
      JsonUtility.FromJsonOverwrite(loadedString, this);
      _isLoaded = true;
      //Debug.Log("LOAD:" + loadedString);
    }
    
    public string SaveString() => 
      JsonUtility.ToJson(this);

    public void Delete() => 
      PlayerPrefs.DeleteKey(name);
  }
}