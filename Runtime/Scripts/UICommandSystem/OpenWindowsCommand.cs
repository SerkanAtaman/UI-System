using UnityEngine;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public class OpenWindowsCommand : UICommand
    {
        [SerializeField]
        [Tooltip("Decide which windows you want to open with this command")]
        private UIWindowReference[] _windows;

        [SerializeField]
        [Tooltip("Decide whether you want to open associated windows immediately or animate them normally")]
        private bool _openImmediately = false;
        
        public OpenWindowsCommand(UIWindow[] windows, bool openImmediate = false, 
            bool solveConflictsAfterOpen = false, bool solveConflictsImmediately = false,
            bool waitForConflicts = true)
        {
            _openImmediately = openImmediate;
            _windows = UIHelper.ConvertToWindowReferenceArray(windows);
        }

        public override UIProccess GetProccess(FlowController flowController)
        {
            UIWindow[] targetWindows = new UIWindow[_windows.Length];

            for (int i = 0; i < _windows.Length; i++)
            {
                targetWindows[i] = flowController.WindowsCollection[_windows[i].WindowID];
            }

            if (targetWindows.IsNullOrEmpty())
            {
                UIDebugger.LogWarning(UIDebugConstants.ARRAY_NULL_EMPTY, " => " + "windows to open");
                return null;
            }

            if (targetWindows.Length == 1) return GetSingleWindowProccess(targetWindows[0], flowController);
            else return GetMultipleWindowProccess(targetWindows, flowController);
        }

        private UIProccess GetSingleWindowProccess(UIWindow window, FlowController flowController)
        {
            var openWindowsProccess = new OpenUIWindowProccess(window, _openImmediately);

            var sequence = new UIProccessSequence();

            sequence.Append(openWindowsProccess);

            return sequence;
        }

        private UIProccess GetMultipleWindowProccess(UIWindow[] windows, FlowController flowController)
        {
            var openWindowsProccess = new OpenMultipleWindowProccess(windows, _openImmediately);

            var sequence = new UIProccessSequence();

            sequence.Append(openWindowsProccess);

            return sequence;
        }
    }
}