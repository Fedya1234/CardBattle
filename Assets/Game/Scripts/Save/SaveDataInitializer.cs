using UnityEngine;
using Game.Scripts.Data.Saves;
using UnityEditor;

#if UNITY_EDITOR
namespace Game.Scripts.Save
{
    /// <summary>
    /// Editor utility to create SaveData asset in Resources
    /// </summary>
    public static class SaveDataInitializer
    {
        [MenuItem("Game/Create Save Data Asset")]
        public static void CreateSaveDataAsset()
        {
            string resourcesPath = "Assets/Resources";
            string saveDataFileName = "SaveData.asset";
            string saveDataPath = $"{resourcesPath}/{saveDataFileName}";
            
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            // Check if asset already exists
            SaveData existingAsset = AssetDatabase.LoadAssetAtPath<SaveData>(saveDataPath);
            if (existingAsset != null)
            {
                Debug.Log($"SaveData asset already exists at {saveDataPath}");
                Selection.activeObject = existingAsset; // Select it in the editor
                return;
            }
            
            // Create new SaveData asset
            SaveData saveData = ScriptableObject.CreateInstance<SaveData>();
            saveData.SetToDefault();
            
            // Save asset to file
            AssetDatabase.CreateAsset(saveData, saveDataPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new asset
            Selection.activeObject = saveData;
            Debug.Log($"Created SaveData asset at {saveDataPath}");
        }
    }
}
#endif