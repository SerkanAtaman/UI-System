using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SeroJob.UiSystem
{
    public class VerticalLayoutWindow : UIWindow
    {
        [SerializeField] protected VerticalLayoutGroup layout;

        public float ExpandLayoutDuration = 0.5f;

        public VerticalLayoutGroup Layout => layout;

        protected VerticalLayoutPagePreset[] defaultPagePresets;

        protected bool blockDimensionChangeCallback = false;

        protected override void Awake()
        {
            base.Awake();

            if (pages == null)
            {
                defaultPagePresets = null;
            }
            else
            {
                defaultPagePresets = new VerticalLayoutPagePreset[pages.Length];
                for (int i = 0; i < pages.Length; i++)
                {
                    if (pages[i] == null) continue;
                    defaultPagePresets[i] = new VerticalLayoutPagePreset(pages[i]);
                    pages[i].HideImmediately();
                    pages[i].gameObject.SetActive(false);
                    defaultPagePresets[i].Parent.gameObject.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            blockDimensionChangeCallback = false;
        }

        protected override void WindowCloseEnded()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    page.OnWindowCloseEnded(this);
                    var preset = GetPresetFor(page);
                    if (preset != null && preset.Parent != null)
                    {
                        preset.Page.HideImmediately();
                        preset.Page.gameObject.SetActive(false);
                        preset.Parent.gameObject.SetActive(false);
                    }
                }
            }
            base.WindowCloseEnded();
        }

        public override void Open(Action callback = null)
        {
            if (State == UIWindowState.Opened || State == UIWindowState.Opening) return;

            gameObject.SetActive(true);
            Canvas.enabled = true;
            windowState = UIWindowState.Opened;
            onWindowAnimatedCallback = null;
            remainingPagesToAnimate = 0;
            UIData.OnWindowOpened.Invoke(this);

            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowOpenEnded(this);
                }
            }

            callback?.Invoke();
        }

        public override void OpenImmediately()
        {
            if (State == UIWindowState.Opened || State == UIWindowState.Opening) return;

            gameObject.SetActive(true);
            Canvas.enabled = true;
            windowState = UIWindowState.Opened;
            onWindowAnimatedCallback = null;
            remainingPagesToAnimate = 0;
            UIData.OnWindowOpened.Invoke(this);

            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowOpenEnded(this);
                }
            }
        }

        public void OpenLayoutPage(UIPage page)
        {
            if (!IsPageValid(page))
            {
                Debug.LogError("Page is not valid", page);
                return;
            }

            var preset = GetPresetFor(page);

            if (preset == null)
            {
                Debug.LogError("Failed to find the preset of given page!", page.gameObject);
                return;
            }

            if (page.PageState == UIPageState.Opening || page.PageState == UIPageState.Opened) return;

            if (State != UIWindowState.Opening && State != UIWindowState.Opened)
                CurrentFlowController.OpenWindow(ID, null, true);

            MovePageToOpen(page, preset);
        }

        private void MovePageToOpen(UIPage page, VerticalLayoutPagePreset preset)
        {
            preset.ActiveTween?.Kill(false);

            if (!preset.Parent.gameObject.activeSelf)
            {
                blockDimensionChangeCallback = true;
                preset.Parent.gameObject.SetActive(true);
                preset.Parent.sizeDelta = new Vector2(((RectTransform)preset.Page.transform).sizeDelta.x, layout.spacing * -1);
                blockDimensionChangeCallback = false;
            }

            if (!IsAnyNextPageOpen(page))
            {
                blockDimensionChangeCallback = true;
                if (!page.gameObject.activeSelf)
                    page.gameObject.SetActive(true);
                page.Open();
                preset.Parent.sizeDelta = ((RectTransform)preset.Page.transform).sizeDelta;
                preset.ActiveTween = null;

                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                blockDimensionChangeCallback = false;
            }
            else
            {
                blockDimensionChangeCallback = true;
                var expandTween = preset.Parent.DOSizeDelta(((RectTransform)preset.Page.transform).sizeDelta, ExpandLayoutDuration);
                expandTween.onUpdate += () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                };
                expandTween.onComplete += () =>
                {
                    preset.ActiveTween = null;
                    if (!page.gameObject.activeSelf) page.gameObject.SetActive(true);
                    page.Open();
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                    blockDimensionChangeCallback = false;
                };

                preset.ActiveTween = expandTween;
                blockDimensionChangeCallback = false;
            }
        }

        public void CloseLayoutPage(UIPage page)
        {
            if (!IsPageValid(page))
            {
                Debug.LogError("Page is not valid", page);
                return;
            }

            var preset = GetPresetFor(page);

            if (preset == null)
            {
                Debug.LogError("Failed to find the preset of given page!", page.gameObject);
                return;
            }

            if (page.PageState == UIPageState.Closing || page.PageState == UIPageState.Closed) return;

            MovePageToClose(page, preset);
        }

        private void MovePageToClose(UIPage page, VerticalLayoutPagePreset preset)
        {
            preset.ActiveTween?.Kill(false);

            var targetSizeDelta = ((RectTransform)preset.Page.transform).sizeDelta;
            targetSizeDelta.y = layout.spacing * -1;

            if (!IsAnyNextPageOpen(page))
            {
                page.Close(() =>
                {
                    blockDimensionChangeCallback = true;
                    preset.Parent.sizeDelta = targetSizeDelta;
                    preset.Page.gameObject.SetActive(false);
                    preset.Parent.gameObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                    blockDimensionChangeCallback = false;
                });
            }
            else
            {
                blockDimensionChangeCallback = true;
                page.Close();

                var duration = page.CloseAnim.GetMaxDuration();
                var tween = preset.Parent.DOSizeDelta(targetSizeDelta, duration).SetDelay(duration / 2f);
                tween.onComplete += () =>
                {
                    blockDimensionChangeCallback = true;
                    preset.ActiveTween = null;
                    preset.Page.gameObject.SetActive(false);
                    preset.Parent.gameObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                    blockDimensionChangeCallback = false;
                };
                tween.onUpdate += () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                };

                preset.ActiveTween = tween;
                blockDimensionChangeCallback = false;
            }
        }

        public void OnRectTransformDimensionsChanged(RectTransform rectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);

            if (blockDimensionChangeCallback || defaultPagePresets == null) return;

            foreach (var preset in defaultPagePresets)
            {
                if (preset == null || preset.Parent != rectTransform) continue;

                if (preset.Page.PageState == UIPageState.Opening) MovePageToOpen(preset.Page, preset);
                else if (preset.Page.PageState == UIPageState.Closing) MovePageToClose(preset.Page, preset);
            }
        }

        private bool IsPageValid(UIPage page)
        {
            if (page == null) return false;

            foreach (var childPage in pages)
            {
                if (childPage == null) continue;
                if (childPage == page) return true;
            }

            return false;
        }

        private VerticalLayoutPagePreset GetPresetFor(UIPage page)
        {
            if (defaultPagePresets == null) return null;

            foreach (var preset in defaultPagePresets)
            {
                if (preset == null || preset.Page == null) continue;
                if (preset.Page == page) return preset;
            }

            return null;
        }

        private UIPage GetNextPage(UIPage page)
        {
            if (pages == null) return null;

            var index = pages.IndexOf(page);

            if (index < 0 || index >= pages.Length - 1) return null;

            return pages[index + 1];
        }

        private bool IsAnyNextPageOpen(UIPage page)
        {
            if (pages == null || page == null) return false;

            var index = pages.IndexOf(page);

            if (index < 0 || index >= pages.Length - 1) return false;

            for (var i = index + 1; i < pages.Length; i++)
            {
                if (pages[i] == null) continue;
                if (pages[i].PageState == UIPageState.Opening || pages[i].PageState == UIPageState.Opened) return true;
            }

            return false;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            layout = GetComponentInChildren<VerticalLayoutGroup>();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}