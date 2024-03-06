using UnityEngine;

namespace SeroJob.UiSystem.ConditionHandling
{
    public class AndroidCondition : BaseCondition
    {
        public override string Definition { get => "Android"; }

        public override bool IsConditionValid()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
    }
}