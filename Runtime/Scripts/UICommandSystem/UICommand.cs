namespace SeroJob.UiSystem
{
    [System.Serializable]
    public abstract class UICommand
    {
        public abstract UIProccess GetProccess(FlowController flowController);
    }
}