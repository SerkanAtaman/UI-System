using NaughtyAttributes;
using UnityEngine;

namespace SeroJob.UiSystem.ConditionHandling
{
    [System.Serializable]
    public class ConditionSerializer
    {
        [SerializeField]
        [Dropdown("Conditions")]
        private string _condition;

        public string[] Conditions => ConditionDatabase.ConditionDefinitions;

        public BaseCondition Condition => ConditionDatabase.GetConditionFromDefinition(_condition);
    }
}