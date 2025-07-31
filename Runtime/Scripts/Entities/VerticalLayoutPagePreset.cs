using DG.Tweening;
using UnityEngine;

namespace SeroJob.UiSystem
{
    public class VerticalLayoutPagePreset
    {
        public UIPage Page;
        public RectTransform Parent;
        public Vector2 DefaultParentSize;

        public Tween ActiveTween;

        public VerticalLayoutPagePreset(UIPage page)
        {
            Page = page;
            Parent = page.transform.parent.GetComponent<RectTransform>();
            DefaultParentSize = Parent.sizeDelta;
            ActiveTween = null;
        }
    }
}