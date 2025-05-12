using UnityEngine;
using UnityEditor;
using System.IO;

namespace SeroJob.UiSystem.Editor
{
    public class UISystemEditor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!didDomainReload) return;

            GetSettingsAsset();
        }

        public static UISettings GetSettingsAsset()
        {
            var settingsGuids = AssetDatabase.FindAssets("t:UISettings");

            if (settingsGuids == null || settingsGuids.Length < 1)
            {
                return CreateSettingsAsset();
            }

            for (int i = 1; i < settingsGuids.Length; i++)
            {
                Debug.LogWarning("Please delete the extra UIsettings asset at: " + AssetDatabase.GUIDToAssetPath(settingsGuids[i]));
            }

            return AssetDatabase.LoadAssetAtPath<UISettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
        }

        public static UISettings CreateSettingsAsset()
        {
            var path = "Assets/Serojob-UI-System/UISettings.asset";
            var settings = ScriptableObject.CreateInstance<UISettings>();
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            AssetDatabase.CreateAsset(settings, path);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);

            Debug.Log("Created UISettings asset at: " + path);

            return settings;
        }
    }
}