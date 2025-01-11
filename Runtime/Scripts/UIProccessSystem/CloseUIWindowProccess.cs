namespace SeroJob.UiSystem
{
    public class CloseUIWindowProccess : UIProccess
    {
        public override string Description => "Close UI Window Proccess";

        private readonly UIWindow _window;

        private readonly bool _closeImmidiately;

        public CloseUIWindowProccess(UIWindow window, bool closeImmidiately)
        {
            OnReworkCompleted = new ProtectedAction<UIProccess>();
            OnWorkCompleted = new ProtectedAction<UIProccess>();
            State = UIProccessState.Unworked;
            _window = window;
            _closeImmidiately = closeImmidiately;
        }

        public override void Work()
        {
            if(State != UIProccessState.Unworked)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_NOT_WORKABLE} => {Description} - {_window.ID}");
                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_STARTED} => {Description} - {_window.ID}");

            State = UIProccessState.Working;

            if (_closeImmidiately)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_COMPLETED_IMMEDIATE} => {Description} - {_window.ID}");

                State = UIProccessState.Worked;

                _window.HideImmediately();
                OnWorkCompleted?.Invoke(this);
            }
            else
            {
                _window.Close(OnWindowClosed);
            }
        }

        public override void Rework()
        {
            if(State != UIProccessState.Worked)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_NOT_REWORKABLE} => {Description} - {_window.ID}");
                return;
            }

            State = UIProccessState.Reworking;

            if (_closeImmidiately)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_REWORK_COMPLETED_IMMEDIATE} => {Description} - {_window.ID}");

                State = UIProccessState.Reworked;

                _window.OpenImmediately();
                OnReworkCompleted?.Invoke(this);
            }
            else
            {
                _window.Open(OnWindowOpened);
            }
        }

        private void OnWindowClosed()
        {
            if (State != UIProccessState.Working)
            {
                var message = "OnWindowClosed callback of CloseUIWindowProccess invoked while proccess is already worked" +
                    "This can be caused by callback is being invoked by multiple times which will cause errors therefore not allowed";

                UIDebugger.LogWarning(message);
                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_COMPLETED} => {Description} - {_window.ID}");

            State = UIProccessState.Worked;

            OnWorkCompleted?.Invoke(this);
        }

        private void OnWindowOpened()
        {
            if(State != UIProccessState.Reworking)
            {
                var message = "OnWindowOpened callback of CloseUIWindowProccess invoked while proccess is already reworked" +
                    "This can be caused by callback is being invoked by multiple times which will cause errors therefore not allowed";
                UIDebugger.LogWarning(message);
                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_REWORK_COMPLETED} => {Description} - {_window.ID}");

            State = UIProccessState.Reworked;

            OnReworkCompleted?.Invoke(this);
        }
    }
}