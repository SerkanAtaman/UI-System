using NaughtyAttributes;
using UnityEngine;

namespace SeroJob.UiSystem
{
    [CreateAssetMenu(menuName = "SeroJob/UiSystem/FlowDatabase")]
    public class FlowDatabase : ScriptableObject
    {
        [SerializeField, ReadOnly] private string _flowName;
        [SerializeField] private bool _hideAllWindows = false;
        [SerializeField] private bool _disableAllWindows = false;
        [SerializeField, ReadOnly] private string[] _windowIDs;

        public string[] WindowIDs
        {
            get
            {
                var array = _windowIDs.Add("None", 0);
                return array.Add("everything", array.Length);
            }
            set 
            {
                _windowIDs = value;
            }
        }

        public string FlowName
        {
            get => _flowName;
            set => _flowName = value;
        }

        public bool HideAllWindows
        {
            get => _hideAllWindows;
            set
            {
                _hideAllWindows = value;

                var flow = UIData.GetRegisteredFlowControllerByName(_flowName);
                if (flow != null)
                {
                    if(_disableAllWindows)flow.SetAllWindowVisibility(false);
                    else flow.SetAllWindowVisibility(!_hideAllWindows);
                }
            }
        }

        public bool DisableAllWindows
        {
            get => _disableAllWindows;
            set
            {
                _disableAllWindows = value;

                var flow = UIData.GetRegisteredFlowControllerByName(_flowName);
                if (flow != null)
                {
                    if(_disableAllWindows) flow.SetAllWindowVisibility(false, true);
                    else flow.SetAllWindowVisibility(!_hideAllWindows);
                }
            }
        }


        public ProtectedAction<UICommand> OnCommandGiven { get; private set; }

        private void OnEnable()
        {
            OnCommandGiven = new();
        }

        public string[] GetRawWindowIds()
        {
            return _windowIDs;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            HideAllWindows = _hideAllWindows;
        }
#endif
    }

}
