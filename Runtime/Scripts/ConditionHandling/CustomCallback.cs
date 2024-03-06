using UnityEngine.Events;

namespace SeroJob.UiSystem.ConditionHandling
{
    [System.Serializable]
    public class CustomCallback
    {
        public float Delay = 0.0f;

        public UnityEvent Callback;

        public void Invoke()
        {
            Callback?.Invoke();
        }
    }
}