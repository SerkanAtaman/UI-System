using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public class ProccessCollection : UIProccess
    {
        public override string Description => "Proccess Collection";

        public readonly int CollectionOrder;

        private readonly List<UIProccess> _collection;

        private int _collectionSize;

        public ProccessCollection(int collectionOrder)
        {
            CollectionOrder = collectionOrder;
            OnReworkCompleted = new ProtectedAction<UIProccess>();
            OnWorkCompleted = new ProtectedAction<UIProccess>();
            _collection = new List<UIProccess>();
        }

        public override void Work()
        {
            if (State != UIProccessState.Unworked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_WORKABLE, $" => {Description}");
                return;
            }

            _collectionSize = _collection.Count;

            if (_collectionSize == 0)
            {
                UIDebugger.LogError(UIDebugConstants.WORKING_EMPTY_PROCCESS, $" => {Description}");

                OnWorkCompleted?.Invoke(this);
                State = UIProccessState.Worked;
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_STARTED, $" => {Description}");

            State = UIProccessState.Working;

            foreach (var item in _collection)
            {
                item.OnWorkCompleted.AddListener(OnCollectionItemWorked);
                item.Work();
            }
        }

        public override void Rework()
        {
            if (State != UIProccessState.Worked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_REWORKABLE, $" => {Description}");
                return;
            }

            _collectionSize = _collection.Count;

            if (_collectionSize == 0)
            {
                UIDebugger.LogError(UIDebugConstants.WORKING_EMPTY_PROCCESS, $" => {Description}");

                OnReworkCompleted?.Invoke(this);
                State = UIProccessState.Reworked;
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_STARTED, $" => {Description}");

            State = UIProccessState.Reworking;

            foreach (var item in _collection)
            {
                item.OnReworkCompleted.AddListener(OnCollectionItemReworked);
                item.Rework();
            }
        }

        public void ExpandCollection(UIProccess proccess)
        {
            _collection.Add(proccess);
        }

        private void OnCollectionItemWorked(UIProccess sender)
        {
            _collectionSize--;
            sender.OnWorkCompleted.RemoveListener(OnCollectionItemWorked);

            if(_collectionSize < 0)
            {
                var message = "The integer _collectionSize of ProccessCollection dropped below zero which means the OnCollectionItemWorked callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";

                UIDebugger.LogWarning(message);
            }

            if(_collectionSize == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Worked;

                OnWorkCompleted.Invoke(this);
            }
        }

        private void OnCollectionItemReworked(UIProccess sender)
        {
            _collectionSize--;
            sender.OnReworkCompleted.RemoveListener(OnCollectionItemReworked);

            if (_collectionSize < 0)
            {
                var message = "The integer _collectionSize of ProccessCollection dropped below zero which means the OnCollectionItemReworked callback invoked more than its needed to be. " +
                    "This is usually ok because UIProcess is clever enough to check it but you should revise your code to prevent this for future cases";

                UIDebugger.LogWarning(message);
            }

            if (_collectionSize == 0)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Reworked;

                OnReworkCompleted.Invoke(this);
            }
        }
    }
}