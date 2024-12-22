using NaughtyAttributes;
using UnityEngine;

namespace SeroJob.UiSystem
{
    [CreateAssetMenu(menuName = "SeroJob/UiSystem/FlowDatabase")]
    public class FlowDatabase : ScriptableObject
    {
        [SerializeField][ReadOnly] private string[] _windowIDs;

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

        public ProtectedAction<UICommand> OnCommandGiven { get; private set; }

        private void OnEnable()
        {
            OnCommandGiven = new();
        }

        public string[] GetRawWindowIds()
        {
            return _windowIDs;
        }
    }
}