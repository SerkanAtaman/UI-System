using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public class ResolveWindowConflictProccess : UIProccess
    {
        public override string Description => "Resolve Window Conflict Proccess";

        private readonly UIWindow[] _conflictedWindows;

        private int _windowToProccessCount;

        private readonly bool _revolveImmediately;

        public ResolveWindowConflictProccess(UIWindow window, List<UIWindow> openedWindows, bool resolveImmediately = false)
        {
            OnReworkCompleted = new ProtectedAction<UIProccess>();
            OnWorkCompleted = new ProtectedAction<UIProccess>();
            _windowToProccessCount = 0;

            _conflictedWindows = UIHelper.GetConflictedWindows(window, openedWindows).ToArray();

            _revolveImmediately = resolveImmediately;
        }

        public override void Work()
        {
            if (State != UIProccessState.Unworked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_WORKABLE, $" => {Description}");
                return;
            }

            State = UIProccessState.Working;

            _windowToProccessCount = _conflictedWindows.Length;

            if(_windowToProccessCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.NO_CONFLICT_WINDOW, $" => {Description} Therefore completing the proccess!");

                State = UIProccessState.Worked;

                OnWorkCompleted?.Invoke(this);
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_STARTED, $" => {Description}!");

            foreach (var target in _conflictedWindows)
            {
                UIDebugger.LogMessage(UIDebugConstants.CONFLICTED_WINDOW_FOUND, $" => {target.name}!");

                if (!_revolveImmediately) target.Close(OnSingleWindowClosed);
                else target.HideImmediately();
            }

            if (_revolveImmediately)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED_IMMEDIATE, $" => {Description}!");

                State = UIProccessState.Worked;

                OnWorkCompleted?.Invoke(this);
            }
        }

        public override void Rework()
        {
            if (State != UIProccessState.Worked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_REWORKABLE, $" => {Description}");
                return;
            }

            State = UIProccessState.Reworking;

            _windowToProccessCount = _conflictedWindows.Length;

            if (_windowToProccessCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.NO_CONFLICT_WINDOW, $" => {Description} Therefore completing the proccess!");

                State = UIProccessState.Reworked;

                OnWorkCompleted?.Invoke(this);
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_STARTED, $" => {Description}");

            foreach (var target in _conflictedWindows)
            {
                UIDebugger.LogMessage(UIDebugConstants.CONFLICTED_WINDOW_FOUND, $" => {target.name}!");

                if (!_revolveImmediately) target.Open(OnSingleWindowOpened);
                else target.OpenImmediately();
            }

            if (_revolveImmediately)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED_IMMEDIATE, $" => {Description}!");

                State = UIProccessState.Reworked;

                OnReworkCompleted?.Invoke(this);
            }
        }

        private void OnSingleWindowClosed()
        {
            _windowToProccessCount--;

            if (_windowToProccessCount < 0)
            {
                var message = "The integer _windowToProccessCount dropped below zero which means the OnSingleWindowClosed callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";

                UIDebugger.LogWarning(message);
            }

            if (_windowToProccessCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED, $" => {Description}!");

                State = UIProccessState.Worked;

                OnWorkCompleted?.Invoke(this);
            }
        }

        private void OnSingleWindowOpened()
        {
            _windowToProccessCount--;

            if(_windowToProccessCount < 0)
            {
                var message = "The integer _windowToProccessCount dropped below zero which means the OnSingleWindowOpened callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";

                UIDebugger.LogWarning(message);
            }

            if(_windowToProccessCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED, $" => {Description}!");

                State = UIProccessState.Reworked;

                OnReworkCompleted?.Invoke(this);
            }
        }
    }
}