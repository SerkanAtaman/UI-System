using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace SeroJob.UiSystem.ConditionHandling
{
    [System.Serializable]
    public class ConditionalButtonCallbacks
    {
        [SerializeField]
        private CustomCallback _buttonClickEvents;

        [SerializeField]
        private CustomCallback _validateEvents;

        [SerializeField]
        private CustomCallback _validateFailedEvents;

        public CustomCallback ButtonClickEvents => _buttonClickEvents;
        public CustomCallback ValidateEvents => _validateEvents;
        public CustomCallback ValidateFailedEvents => _validateFailedEvents;
    }
}