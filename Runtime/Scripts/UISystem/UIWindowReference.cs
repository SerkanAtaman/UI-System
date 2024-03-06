using UnityEngine;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public class UIWindowReference
    {
        [SerializeField] private string _windowID;

        [SerializeField] private FlowDatabase _database = null;

        public UIWindowReference(string windowID)
        {
            _windowID = windowID;
        }

        public string WindowID => _windowID;
        public FlowDatabase Database => _database;
    }
}