using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace SeroJob.UiSystem.ConditionHandling
{
    [System.Serializable]
    public class ConditionGroup
    {
        public string Name;

        [SerializeField]
        private ConditionSerializer[] _conditions;

        [SerializeField]
        private ConditionalButtonCallbacks _callbacks;

        public ConditionalButtonCallbacks Callbacks => _callbacks;

        public bool IsConditionsValid()
        {
            int validConditionCount = 0;

            foreach (var condition in _conditions)
            {
                BaseCondition cond = condition.Condition;

                if (cond.IsConditionValid()) validConditionCount++;
            }

            return validConditionCount == _conditions.Length;
        }
    }
}