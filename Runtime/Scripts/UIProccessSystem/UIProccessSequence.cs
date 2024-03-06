using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public class UIProccessSequence : UIProccess
    {
        public override string Description => "UI Proccess Sequence";

        private readonly List<ProccessCollection> _sequenceCollections;

        private int _maxItemOrder;
        private int _currentItemOrder;

        public UIProccessSequence()
        {
            State = UIProccessState.Unworked;
            OnReworkCompleted = new ProtectedAction<UIProccess>();
            OnWorkCompleted = new ProtectedAction<UIProccess>();
            _sequenceCollections = new List<ProccessCollection>();
            _currentItemOrder = 0;
            _maxItemOrder = 0;
        }

        public override void Work()
        {
            if (State != UIProccessState.Unworked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_WORKABLE, $" => {Description}");
                return;
            }

            if(_sequenceCollections.Count == 0)
            {
                UIDebugger.LogError(UIDebugConstants.WORKING_EMPTY_PROCCESS, $" => {Description}");

                OnWorkCompleted?.Invoke(this);
                State = UIProccessState.Worked;
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_STARTED, $" => {Description}");

            State = UIProccessState.Working;
            _currentItemOrder = 0;

            WorkOnProccessCollection(GetCollectionByOrder(_currentItemOrder));
        }

        public override void Rework()
        {
            if (State != UIProccessState.Worked)
            {
                UIDebugger.LogWarning(UIDebugConstants.PROCCESS_NOT_REWORKABLE, $" => {Description}");
                return;
            }

            if (_sequenceCollections.Count == 0)
            {
                UIDebugger.LogError(UIDebugConstants.WORKING_EMPTY_PROCCESS, $" => {Description}");

                OnReworkCompleted?.Invoke(this);
                State = UIProccessState.Reworked;
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_STARTED, $" => {Description}");

            State = UIProccessState.Reworking;

            _currentItemOrder = _maxItemOrder - 1;

            ReworkOnProccessCollection(GetCollectionByOrder(_currentItemOrder));
        }

        /// <summary>
        /// Add the given UIProccess to the end of the current sequence
        /// </summary>
        public void Append(UIProccess proccess)
        {
            UIDebugger.LogMessage(UIDebugConstants.APPEND_PROCCESS_TO_SEQUENCE, $" => {proccess.Description}");

            var collection = new ProccessCollection(_maxItemOrder);
            collection.ExpandCollection(proccess);

            _sequenceCollections.Add(collection);

            _maxItemOrder++;
        }

        public void Join(UIProccess proccess)
        {
            if(_sequenceCollections.Count == 0)
            {
                Append(proccess);
            }
            else
            {
                UIDebugger.LogMessage(UIDebugConstants.JOIN_PROCCESS_TO_SEQUENCE, $" => {proccess.Description}");

                int targetOrder = _maxItemOrder - 1;

                var collection = GetCollectionByOrder(targetOrder);
                collection.ExpandCollection(proccess);
            }
        }

        private ProccessCollection GetCollectionByOrder(int order)
        {
            ProccessCollection result = null;

            foreach(var item in _sequenceCollections)
            {
                if(item.CollectionOrder == order)
                {
                    result = item;
                    break;
                }
            }

            return result;
        }

        private void WorkOnProccessCollection(ProccessCollection collection)
        {
            collection.OnWorkCompleted.AddListener(ContinueToWorkOnCollections);
            collection.Work();
        }

        private void ContinueToWorkOnCollections(UIProccess sender)
        {
            sender.OnWorkCompleted.RemoveListener(ContinueToWorkOnCollections);

            _currentItemOrder++;

            if(_currentItemOrder == _maxItemOrder)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_WORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Worked;

                OnWorkCompleted?.Invoke(this);

                return;
            }

            if(_currentItemOrder > _maxItemOrder)
            {
                UIDebugger.LogWarning("CurrentItemOrder is greater then MaxItemOrder which is a concern");
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.CONTINUE_WORK_ON_COLLECTION, $" => {Description}");

            WorkOnProccessCollection(GetCollectionByOrder(_currentItemOrder));
        }

        private void ReworkOnProccessCollection(ProccessCollection collection)
        {
            collection.OnReworkCompleted.AddListener(ContinueToReworkOnCollections);
            collection.Rework();
        }

        private void ContinueToReworkOnCollections(UIProccess sender)
        {
            sender.OnReworkCompleted.RemoveListener(ContinueToReworkOnCollections);

            _currentItemOrder--;

            if (_currentItemOrder == -1)
            {
                UIDebugger.LogMessage(UIDebugConstants.PROCCESS_REWORK_COMPLETED, $" => {Description}");

                State = UIProccessState.Reworked;

                OnReworkCompleted?.Invoke(this);

                return;
            }

            if (_currentItemOrder < -1)
            {
                UIDebugger.LogWarning("CurrentItemOrder is lesser then zero which is a concern");
                return;
            }

            UIDebugger.LogMessage(UIDebugConstants.CONTINUE_REWORK_ON_COLLECTION, $" => {Description}");

            ReworkOnProccessCollection(GetCollectionByOrder(_currentItemOrder));
        }
    }
}