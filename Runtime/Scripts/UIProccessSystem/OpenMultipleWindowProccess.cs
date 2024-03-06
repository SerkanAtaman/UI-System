namespace SeroJob.UiSystem
{
    public class OpenMultipleWindowProccess : UIProccess
    {
        public override string Description => "Open Multiple Windows Proccess";

        private readonly UIWindow[] _windows;

        private readonly bool _openImmidiate;

        private int _windowToOpenCount;

        public OpenMultipleWindowProccess(UIWindow[] windows, bool openImmidiate = false)
        {
            _openImmidiate = openImmidiate;
            _windows = windows;

            OnWorkCompleted = new ProtectedAction<UIProccess>();
            OnReworkCompleted = new ProtectedAction<UIProccess>();
        }

        public override void Work()
        {
            if (State != UIProccessState.Unworked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_WORKABLE, $" => {Description}");
                return;
            }

            if (_windows.IsNullOrEmpty())
            {
                UIDebugger.LogWarning(UIDebugConstants.ARRAY_NULL_EMPTY, $" => {Description} therefore immidiately completing the work proccess");

                State = UIProccessState.Worked;
                OnWorkCompleted?.Invoke(this);
                return;
            }

            _windowToOpenCount = _windows.Length;

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_STARTED, $" => {Description}");

            State = UIProccessState.Working;

            foreach (var window in _windows)
            {
                UIDebugger.LogMessage(UIDebugConstants.OPENING_WINDOW, $" => {window.name} => {Description}");

                if (_openImmidiate) window.OpenImmediately();
                else window.Open(OnSingleWindowOpened);
            }

            if (_openImmidiate)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED_IMMEDIATE, $" => {Description}");

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

            if (_windows.IsNullOrEmpty())
            {
                UIDebugger.LogWarning(UIDebugConstants.ARRAY_NULL_EMPTY, $" => {Description} therefore immidiately completing the rework proccess");

                State = UIProccessState.Reworked;
                OnReworkCompleted?.Invoke(this);
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_STARTED, $" => {Description}");

            _windowToOpenCount = _windows.Length;
            State = UIProccessState.Reworking;

            foreach (var window in _windows)
            {
                UIDebugger.LogMessage(UIDebugConstants.OPENING_WINDOW, $" => {window.name} => {Description}");

                if (_openImmidiate) window.HideImmediately();
                else window.Close(OnSingleWindowClosed);
            }

            if (_openImmidiate)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED_IMMEDIATE, $" => {Description}");

                State = UIProccessState.Reworked;
                OnReworkCompleted?.Invoke(this);
            }
        }

        private void OnSingleWindowOpened()
        {
            _windowToOpenCount--;

            if (_windowToOpenCount < 0)
            {
                var message = "The integer _windowToOpenCount dropped below zero which means the OnSingleWindowOpened callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";
                
                UIDebugger.LogWarning(message);
            }

            if (_windowToOpenCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Worked;
                OnWorkCompleted?.Invoke(this);
            }
        }

        private void OnSingleWindowClosed()
        {
            _windowToOpenCount--;

            if (_windowToOpenCount < 0)
            {
                var message = "The integer _windowToOpenCount dropped below zero which means the OnSingleWindowClosed callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";
                UIDebugger.LogWarning(message);
            }

            if (_windowToOpenCount == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Reworked;
                OnReworkCompleted?.Invoke(this);
            }
        }
    }
}