using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SeroJob.UiSystem
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformTracker : UIBehaviour
    {
        public UnityEvent<RectTransform> OnDimensionsChange;

        bool _enable = false;

        protected override void OnEnable()
        {
            _enable = true;
        }

        protected override void OnDisable()
        {
            _enable = false;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (_enable)
                OnDimensionsChange?.Invoke((RectTransform)transform);
        }

        public void TransferSizeDelta(RectTransform other)
        {
            ((RectTransform)transform).sizeDelta = other.sizeDelta;
        }
    }
}