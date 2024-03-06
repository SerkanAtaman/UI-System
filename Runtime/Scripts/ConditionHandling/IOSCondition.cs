using UnityEngine;

namespace SeroJob.UiSystem.ConditionHandling
{
    public class IOSCondition : BaseCondition
    {
        public override string Definition { get => "Ios"; }

        public override bool IsConditionValid()
        {
#if UNITY_IOS
            return true;
#else
            return false;
#endif
        }
    }
}