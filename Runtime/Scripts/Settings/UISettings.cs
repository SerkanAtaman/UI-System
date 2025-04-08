using UnityEngine;

namespace SeroJob.UiSystem
{
    [CreateAssetMenu(menuName = "SeroJob/UiSystem/Settings")]
    public class UISettings : ScriptableObject
    {
        [SerializeField] private bool isDebugEnabled = true;
        [SerializeField] private float _uiScale = 1.0f;

        public bool IsDebugEnabled => isDebugEnabled;
        public float UIScale
        {
            get => _uiScale;
            set
            {
                _uiScale = Mathf.Clamp(value, 0.1f, 10);

                if (UIData.RegisteredFlowControllers != null)
                {
                    foreach (var flow in UIData.RegisteredFlowControllers)
                    {
                        if (flow == null) continue;
                        flow.SetAllScalableWindowsScale(_uiScale);
                    }
                }
            }
        }

        public void ApplySettings()
        {
            UIDebugger.DebugEnabled = isDebugEnabled;
            UIScale = _uiScale;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _uiScale = Mathf.Clamp(_uiScale, 0.1f, 10);

            ApplySettings();
        }
#endif
    }
}
