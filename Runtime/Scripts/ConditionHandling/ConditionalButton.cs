using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SeroJob.UiSystem.ConditionHandling
{
    [RequireComponent(typeof(Button))]
    public class ConditionalButton : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private string _currentCondition;

        [SerializeField]
        private ConditionalButtonInitializeTime _initializeTime;

        [Space(10f)]
        [SerializeField]
        private ConditionGroup[] _conditions;

        private int _validatedConditionCount;

        private void Awake()
        {
            if(_initializeTime == ConditionalButtonInitializeTime.Awake)
            {
                Init();
            }
        }

        private void OnEnable()
        {
            if (_initializeTime == ConditionalButtonInitializeTime.OnEnable)
            {
                Init();
            }
        }

        private void Start()
        {
            if (_initializeTime == ConditionalButtonInitializeTime.Start)
            {
                Init();
            }
        }

        private void Init()
        {
            _validatedConditionCount = 0;

            foreach(var condition in _conditions)
            {
                if (condition.IsConditionsValid())
                {
                    _validatedConditionCount++;

                    if(_validatedConditionCount > 1)
                    {
                        UIDebugger.LogWarning("Multiple valid conditions detected at the same time! This is not allowed!");
                        continue;
                    }

                    _currentCondition = condition.Name;

                    var button = GetComponent<Button>();

                    button.onClick.RemoveAllListeners();

                    button.onClick.AddListener(() => condition.Callbacks.ButtonClickEvents.Invoke());

                    if (condition.Callbacks.ValidateEvents.Delay < 0.05f) condition.Callbacks.ValidateEvents.Invoke();
                    else StartCoroutine(Delay(condition.Callbacks.ValidateEvents.Delay, () => condition.Callbacks.ValidateEvents.Invoke()));
                }
                else
                {
                    condition.Callbacks.ValidateFailedEvents.Invoke();
                }
            }
        }

        private IEnumerator Delay(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);

            callback();
        }
    }
}