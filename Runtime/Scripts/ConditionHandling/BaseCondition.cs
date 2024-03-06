using NaughtyAttributes;
using System;
using UnityEngine;

namespace SeroJob.UiSystem.ConditionHandling
{
    public abstract class BaseCondition
    {
        public abstract string Definition { get; }

        public abstract bool IsConditionValid();
    }
}