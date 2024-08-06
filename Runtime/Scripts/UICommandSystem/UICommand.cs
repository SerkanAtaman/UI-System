namespace SeroJob.UiSystem
{
    [System.Serializable]
    public abstract class UICommand
    {
        [UnityEngine.SerializeField] protected bool queueCommand = true;
        public abstract UIProccess GetProccess(FlowController flowController);

        public bool QueueCommand => queueCommand;
    }
}