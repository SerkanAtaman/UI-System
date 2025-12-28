using UnityEngine;
using UnityEditor;
using System.IO;

#if SEROJOB_EDITOR_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

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
            UISettings result;

            if (settingsGuids == null || settingsGuids.Length < 1)
            {
                result = CreateSettingsAsset();
            }
            else
            {
                for (int i = 1; i < settingsGuids.Length; i++)
                {
                    Debug.LogWarning("Please delete the extra UIsettings asset at: " + AssetDatabase.GUIDToAssetPath(settingsGuids[i]));
                }

                result = AssetDatabase.LoadAssetAtPath<UISettings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            }

#if SEROJOB_EDITOR_ADDRESSABLES
            if (result != null) AssignSettingsAssetToAddressables(result);
#endif

            return result;
        }

        public static UISettings CreateSettingsAsset()
        {
#if SEROJOB_EDITOR_ADDRESSABLES
            var path = "Assets/Serojob-UI-System/UISettings.asset";
            var settings = ScriptableObject.CreateInstance<UISettings>();
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            AssetDatabase.CreateAsset(settings, path);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);

            Debug.Log("Created UISettings asset at: " + path);

            return settings;
#else
            var path = "Assets/Resources/Serojob-UI-System/UISettings.asset";
            var settings = ScriptableObject.CreateInstance<UISettings>();
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            AssetDatabase.CreateAsset(settings, path);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);

            Debug.Log("Created UISettings asset at: " + path);

            return settings;
#endif
        }

#if SEROJOB_EDITOR_ADDRESSABLES
        public static void AssignSettingsAssetToAddressables(UISettings settings)
        {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null)
            {
                Debug.LogError("addressable settings is nul");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(settings);
            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var currentEntry = addressableSettings.FindAssetEntry(assetGuid);

            if (currentEntry != null)
            {
                AssignSettingsAddressableLabel(currentEntry);
                return;
            }

            var targetGroupName = "Serojob-UISystem";
            var group = addressableSettings.FindGroup(targetGroupName);

            if (group == null)
            {
                group = addressableSettings.CreateGroup(targetGroupName,
                    false, true, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            currentEntry = addressableSettings.CreateOrMoveEntry(assetGuid, group, true);

            AssignSettingsAddressableLabel(currentEntry);

            EditorUtility.SetDirty(addressableSettings);
            AssetDatabase.SaveAssetIfDirty(addressableSettings);
        }

        public static void AssignSettingsAddressableLabel(AddressableAssetEntry addressableEntry)
        {
            if (addressableEntry == null || addressableEntry.labels.Contains("SerojobUISystemSettings")) return;

            addressableEntry.SetLabel("SerojobUISystemSettings", true, true);

            EditorUtility.SetDirty(addressableEntry.parentGroup);
            EditorUtility.SetDirty(addressableEntry.parentGroup.Settings);
            AssetDatabase.SaveAssetIfDirty(addressableEntry.parentGroup);
        }
#endif
    }
}