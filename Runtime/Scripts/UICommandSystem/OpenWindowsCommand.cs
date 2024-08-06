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

        [SerializeField]
        [Tooltip("Conflict resolving prevents conflicted windows to be opened at the same time by closing windows that cause conflict." + 
                   "Decide whether to do this task before or after opening new windows")]
        private bool _solveConflictsAfterOpen = false;

        [SerializeField]
        [Tooltip("Decide whether to close conflicted windows immediately")]
        private bool _solveConflictsImmediately = false;

        [SerializeField]
        private bool _waitForConflicts = true;
        
        public OpenWindowsCommand(UIWindow[] windows, bool openImmediate = false, 
            bool solveConflictsAfterOpen = false, bool solveConflictsImmediately = false,
            bool waitForConflicts = true)
        {
            queueCommand = true;
            _solveConflictsAfterOpen = solveConflictsAfterOpen;
            _openImmediately = openImmediate;
            _waitForConflicts = waitForConflicts;

            _windows = UIHelper.ConvertToWindowReferenceArray(windows);
            _solveConflictsImmediately = solveConflictsImmediately;
        }

        public override UIProccess GetProccess(FlowController flowController)
        {
            UIWindow[] targetWindows = new UIWindow[_windows.Length];

            for (int i = 0; i < _windows.Length; i++)
            {
                targetWindows[i] = flowController.WindowsCollection[_windows[i].WindowID];
            }

            UIWindow[] openableWindows = UIHelper.GetOpenableWindows(targetWindows);

            if (openableWindows.IsNullOrEmpty())
            {
                UIDebugger.LogWarning(UIDebugConstants.ARRAY_NULL_EMPTY, " => " + "windows to open");
                return null;
            }

            if (UIHelper.DoesWindowsConflict(openableWindows))
            {
                UIDebugger.LogError(UIDebugConstants.MULTIPLE_WINDOWS_CONFLICT, " : Openable Windows");
                return null;
            }

            if (openableWindows.Length == 1) return GetSingleWindowProccess(openableWindows[0], flowController);
            else return GetMultipleWindowProccess(openableWindows, flowController);
        }

        private UIProccess GetSingleWindowProccess(UIWindow window, FlowController flowController)
        {
            var resolveConflictsProccess = new ResolveWindowConflictProccess(window, flowController.OpenedWindows, _solveConflictsImmediately);
            var openWindowsProccess = new OpenUIWindowProccess(window, _openImmediately);

            var sequence = new UIProccessSequence();

            if (!_waitForConflicts)
            {
                sequence.Append(resolveConflictsProccess);
                sequence.Join(openWindowsProccess);
            }
            else
            {
                if (_solveConflictsAfterOpen)
                {
                    sequence.Append(openWindowsProccess);
                    sequence.Append(resolveConflictsProccess);
                }
                else
                {
                    sequence.Append(resolveConflictsProccess);
                    sequence.Append(openWindowsProccess);
                }
            }

            return sequence;
        }

        private UIProccess GetMultipleWindowProccess(UIWindow[] windows, FlowController flowController)
        {
            var resolveConflictsProccess = new ResolveMultipleWindowConflictProccess(windows, flowController.OpenedWindows, _solveConflictsImmediately);
            var openWindowsProccess = new OpenMultipleWindowProccess(windows, _openImmediately);

            var sequence = new UIProccessSequence();

            if (_solveConflictsAfterOpen)
            {
                sequence.Append(openWindowsProccess);
                sequence.Append(resolveConflictsProccess);
            }
            else
            {
                sequence.Append(resolveConflictsProccess);
                sequence.Append(openWindowsProccess);
            }

            return sequence;
        }
    }
}