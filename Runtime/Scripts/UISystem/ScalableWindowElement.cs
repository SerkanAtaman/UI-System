using UnityEngine;

namespace SeroJob.UiSystem
{
    [System.Serializable]
    public class ScalableWindowElement
    {
        [System.Serializable]
        public struct ScaleOption
        {
            public float MinScaleX;
            public float MaxScaleX;
            public float MinScaleY;
            public float MaxScaleY;
        }

        public RectTransform TargetRectTransform;
        public ScaleOption ScaleOptions;

        public void ApplyScale(float scale)
        {
            if (TargetRectTransform == null) return;

            var targetScaleX = Mathf.Clamp(scale, ScaleOptions.MinScaleX, ScaleOptions.MaxScaleX);
            var targetScaleY = Mathf.Clamp(scale, ScaleOptions.MinScaleY, ScaleOptions.MaxScaleY);

            TargetRectTransform.localScale = new Vector3(targetScaleX, targetScaleY, 1);
        }
    }
}