namespace SeroJob.UiSystem
{
    public abstract class UIProccess
    {
        public UIProccessState State {  get; protected set; }

        public virtual string Description { get; }

        public ProtectedAction<UIProccess> OnWorkCompleted { get; protected set; }
        public ProtectedAction<UIProccess> OnReworkCompleted { get; protected set; }

        public abstract void Work();

        public abstract void Rework();
    }
}