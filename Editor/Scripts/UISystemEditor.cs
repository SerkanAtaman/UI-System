using UnityEngine;
using UnityEditor;

namespace SeroJob.UiSystem.Editor
{
    [InitializeOnLoad]
    public static class UISystemEditor
    {
        private const string SETTINGS_ASSET_PATH = "Assets/Resources/UISystem/UISettings.asset";
        private const string RESOURCES_FOLDER_PATH = "Assets/Resources";
        private const string SETTINGS_FOLDER_PATH = "Assets/Resources/UISystem";

        private static bool ResourcesFolderExist => AssetDatabase.IsValidFolder(RESOURCES_FOLDER_PATH);
        private static bool SettingsFolderExist => AssetDatabase.IsValidFolder(SETTINGS_FOLDER_PATH);
        private static bool SettingsAssetsExist
        {
            get
            {
                UISettings asset = AssetDatabase.LoadAssetAtPath<UISettings>(SETTINGS_ASSET_PATH);

                return asset != null;
            }
        }

        static UISystemEditor()
        {
            CreateSettingsAssetIfNotExist();
        }

        [MenuItem("Window/UISystem/CreateSettings")]
        public static void CreateSettingsAsset()
        {
            if (SettingsAssetsExist)
            {
                UIDebugger.LogWarning("UISystem Settings Asset is already exist under the resources folder");
                return;
            }

            UISettings settingsInstance = ScriptableObject.CreateInstance<UISettings>();

            if (!ResourcesFolderExist) CreateResourcesFolder();
            if (!SettingsFolderExist) CreateSettingsFolder();

            AssetDatabase.CreateAsset(settingsInstance, SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateResourcesFolder()
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        private static void CreateSettingsFolder()
        {
            AssetDatabase.CreateFolder("Assets/Resources", "UISystem");
        }

        private static void CreateSettingsAssetIfNotExist()
        {
            if (!SettingsAssetsExist)
            {
                UIDebugger.LogMessage("Failed to find UISettings asset in the resources folder. Creating a new one!");

                CreateSettingsAsset();
            }
        }
    }
}