using System;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public abstract class UICommand : IDisposable
    {
        [UnityEngine.SerializeField] protected bool queueCommand = true;
        public abstract UIProccess GetProccess(FlowController flowController);

        public Action OnCompleted;

        public bool QueueCommand => queueCommand;

        public void Dispose()
        {
            OnCompleted = null;
        }
    }
}