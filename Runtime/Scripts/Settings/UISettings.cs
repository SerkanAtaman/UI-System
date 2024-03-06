using NaughtyAttributes;
using UnityEngine;

namespace SeroJob.UiSystem
{
    [CreateAssetMenu(menuName = "SeroJob/UiSystem/Settings")]
    public class UISettings : ScriptableObject
    {
        [SerializeField] [OnValueChanged("OnDebugValueChanged")] private bool isDebugEnabled = true;

        public void ApplySettings()
        {
            UIDebugger.DebugEnabled = isDebugEnabled;
        }

        #region Editor Methods
#if UNITY_EDITOR

        private void OnDebugValueChanged()
        {
            UIDebugger.DebugEnabled = isDebugEnabled;
        }

#endif
        #endregion
    }
}