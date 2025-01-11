namespace SeroJob.UiSystem
{
    public class OpenUIWindowProccess : UIProccess
    {
        public override string Description => "Open UI Window Proccess";

        private readonly UIWindow _window;

        private readonly bool _openImmidiately;

        public OpenUIWindowProccess(UIWindow window, bool openImmidiate)
        {
            State = UIProccessState.Unworked;
            _openImmidiately = openImmidiate;
            _window = window;
            OnReworkCompleted = new ProtectedAction<UIProccess>();
            OnWorkCompleted = new ProtectedAction<UIProccess>();
        }

        public override void Work()
        {
            if (State != UIProccessState.Unworked)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_NOT_WORKABLE} => {Description} - {_window.ID}");
                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_STARTED} => {Description} - {_window.ID}");

            State = UIProccessState.Working;

            if (_openImmidiately)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_COMPLETED_IMMEDIATE} => {Description} - {_window.ID}");

                State = UIProccessState.Worked;

                _window.OpenImmediately();
                OnWorkCompleted?.Invoke(this);
            }
            else
            {
                _window.Open(OnWindowOpened);
            }
        }

        public override void Rework()
        {
            if (State != UIProccessState.Worked)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_NOT_REWORKABLE} => {Description} - {_window.ID}");
                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_REWORK_STARTED} => {Description} - {_window.ID}");

            State = UIProccessState.Reworking;

            if (_openImmidiately)
            {
                UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_REWORK_COMPLETED_IMMEDIATE} => {Description} - {_window.ID}");

                State = UIProccessState.Reworking;

                _window.HideImmediately();
                OnReworkCompleted?.Invoke(this);
            }
            else
            {
                _window.Close(OnWindowClosed);
            }
        }

        private void OnWindowOpened()
        {
            if (State != UIProccessState.Working)
            {
                var message = "OnWindowOpened callback of OpenUIWindowProccess invoked while proccess is already worked" +
                    "This can be caused by callback is being invoked by multiple times which will cause errors therefore not allowed";

                UIDebugger.LogWarning(message);

                return;
            }

            UIDebugger.LogMessage($"{UIDebugConstants.PROCCESS_WORK_COMPLETED} => {Description} - {_window.ID}");
            State = UIProccessState.Worked;

            OnWorkCompleted?.Invoke(this);
        }

        private void OnWindowClosed()
        {
            if (State != UIProccessState.Working)
            {
                var message = "OnWindowClosed callback of OpenUIWindowProccess invoked while proccess is already reworked" +
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