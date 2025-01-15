using System;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public abstract class UICommand : IDisposable
    {
        public abstract UIProccess GetProccess(FlowController flowController);

        public Action OnCompleted;

        public void Dispose()
        {
            OnCompleted = null;
        }
    }
}