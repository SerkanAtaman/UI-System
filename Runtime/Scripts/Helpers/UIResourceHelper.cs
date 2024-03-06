using UnityEngine;

namespace SeroJob.UiSystem
{
    public static class UIResourceHelper
    {
        public static UISettings Settings { get; private set; }

        public static UISettings GetSettings()
        {
            Settings = Resources.Load<UISettings>("UISystem/UISettings");

            if(Settings == null)
            {
                Debug.LogError("Failed to load UISettings asset from Resources folder. Make sure you have UISettings asset at 'Assets/Resources/UISystem/UISettings.asset'");

                return null;
            }

            return Settings;
        }

        public static bool TryLoadSettings()
        {
            bool result = false;

            Settings = Resources.Load<UISettings>("UISystem/UISettings");

            if (Settings == null)
            {
                Debug.LogError("Failed to load UISettings asset from Resources folder. Make sure you have UISettings asset at 'Assets/Resources/UISystem/UISettings.asset'");
            }
            else
            {
                result = true;
            }

            return result;
        }

        public static void ReleaseSettings()
        {
            Settings = null;
        }
    }
}