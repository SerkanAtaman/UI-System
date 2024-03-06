using UnityEngine;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public class CloseWindowsCommand : UICommand
    {
        [SerializeField]
        [Tooltip("Decide which windows you want to close with this command")]
        private UIWindowReference[] _windows;

        [SerializeField]
        [Tooltip("Decide whether you want to close associated windows immediately or animate them normally")]
        private bool _closeImmediately = false;
        
        public CloseWindowsCommand(UIWindow[] windows, bool closeImmediately = false, bool solveConflictsAfterOpen = false)
        {
            _closeImmediately = closeImmediately;

            _windows = UIHelper.ConvertToWindowReferenceArray(windows);
        }

        public override UIProccess GetProccess(FlowController flowController)
        {
            UIWindow[] targetWindows = new UIWindow[_windows.Length];

            for (int i = 0; i < _windows.Length; i++)
            {
                targetWindows[i] = flowController.WindowsCollection[_windows[i].WindowID];
            }

            UIWindow[] closableWindows = UIHelper.GetClosableWindows(targetWindows);

            if (closableWindows.IsNullOrEmpty())
            {
                UIDebugger.LogWarning(UIDebugConstants.ARRAY_NULL_EMPTY, " => " + "windows to close");
                return null;
            }

            if(closableWindows.Length == 1) return GetSingleWindowProccess(closableWindows[0]);
            else return GetMultipleWindowProccess(closableWindows);
        }

        private UIProccess GetSingleWindowProccess(UIWindow window)
        {
            var closeWindowsProccess = new CloseUIWindowProccess(window, _closeImmediately);

            var sequence = new UIProccessSequence();

            sequence.Append(closeWindowsProccess);

            return sequence;
        }

        private UIProccess GetMultipleWindowProccess(UIWindow[] windows)
        {
            var closeWindowsProccess = new CloseMultipleWindowProccess(windows, _closeImmediately);

            var sequence = new UIProccessSequence();

            sequence.Append(closeWindowsProccess);

            return sequence;
        }
    }
}