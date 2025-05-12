using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

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
            UISettings result = null;

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

            AssignSettingsAssetToAddressables(result);

            return result;
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
                    false, true, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            var entry = addressableSettings.CreateOrMoveEntry(assetGuid, group, true);
            entry.address = settings.name;

            AssignSettingsAddressableLabel(currentEntry);

            EditorUtility.SetDirty(addressableSettings);
            AssetDatabase.SaveAssetIfDirty(addressableSettings);
        }

        public static void AssignSettingsAddressableLabel(AddressableAssetEntry addressableEntry)
        {
            if (addressableEntry == null) return;

            if (addressableEntry.labels == null || addressableEntry.labels.Count < 1)
            {
                addressableEntry.SetLabel("SerojobUISystemSettings", true);
            }
            else if (addressableEntry.labels.Count > 1)
            {
                addressableEntry.labels.Clear();
                addressableEntry.SetLabel("SerojobUISystemSettings", true);
            }
            else if (!addressableEntry.labels.Contains("SerojobUISystemSettings"))
            {
                addressableEntry.labels.Clear();
                addressableEntry.SetLabel("SerojobUISystemSettings", true);
            }
            else
            {
                return;
            }

            EditorUtility.SetDirty(addressableEntry.parentGroup);
            EditorUtility.SetDirty(addressableEntry.parentGroup.Settings);
            AssetDatabase.SaveAssetIfDirty(addressableEntry.parentGroup);
        }
    }
}