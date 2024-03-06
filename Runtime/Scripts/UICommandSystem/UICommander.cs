using UnityEngine;
using NaughtyAttributes;

namespace SeroJob.UiSystem
{
    public class UICommander : MonoBehaviour, IFlowProvider
    {
        [SerializeField]
        [InfoBox("Decide what type of command you want to give to the UIManager")]
        [Dropdown("Command")]
        [OnValueChanged("OnCommandValueChanged")]
        private string _commandType;

        [SerializeReference] private UICommand _command;

        [SerializeField]
        [Foldout("References")]
        private FlowDatabase _flowDatabase;

        public string[] Command => new string[] { "OpenWindow", "CloseWindow" };

        public void GiveCommand()
        {
            _flowDatabase.OnCommandGiven.Invoke(_command);
        }

        public FlowDatabase GetFlowDatabase()
        {
            return _flowDatabase;
        }

        #region Editor Utilities
#if UNITY_EDITOR

        //private string[] Command => UIData.Commands;

        private void OnCommandValueChanged()
        {
            if (_commandType.Equals("OpenWindow"))
            {
                _command = new OpenWindowsCommand(new UIWindow[0]);
            }
            else if (_commandType.Equals("CloseWindow"))
            {
                _command = new CloseWindowsCommand(new UIWindow[0]);
            }
        }

#endif
        #endregion
    }
}