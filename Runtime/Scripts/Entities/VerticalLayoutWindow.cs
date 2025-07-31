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

        protected VerticalLayoutPagePreset[] defaultPagePresets;

        public UIPage TargetEditorPage;

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

        protected override void WindowCloseEnded()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
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

            preset.ActiveTween?.Kill(false);

            if (!preset.Page.gameObject.activeSelf)
            {
                preset.Parent.sizeDelta = new Vector2(preset.DefaultParentSize.x, layout.spacing * -1);
                preset.Parent.gameObject.SetActive(true);
            }

            if (!IsAnyNextPageOpen(page))
            {
                preset.Parent.sizeDelta = preset.DefaultParentSize;
                preset.ActiveTween = null;
                if (!page.gameObject.activeSelf) page.gameObject.SetActive(true);
                page.Open();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
            }
            else
            {
                var expandTween = preset.Parent.DOSizeDelta(preset.DefaultParentSize, ExpandLayoutDuration);
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
                };

                preset.ActiveTween = expandTween;
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

            preset.ActiveTween?.Kill(false);
            
            var targetSizeDelta = preset.DefaultParentSize;
            targetSizeDelta.y = layout.spacing * -1;

            if (!IsAnyNextPageOpen(page))
            {
                page.Close(() =>
                {
                    preset.Parent.sizeDelta = targetSizeDelta;
                    preset.Page.gameObject.SetActive(false);
                    preset.Parent.gameObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                });
            }
            else
            {
                page.Close();

                var duration = page.CloseAnim.GetMaxDuration();
                var tween = preset.Parent.DOSizeDelta(targetSizeDelta, duration).SetDelay(duration / 2f);
                tween.onComplete += () =>
                {
                    preset.ActiveTween = null;
                    preset.Page.gameObject.SetActive(false);
                    preset.Parent.gameObject.SetActive(false);
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                };
                tween.onUpdate += () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                };

                preset.ActiveTween = tween;
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

        [NaughtyAttributes.Button()]
        public void TestOpenPage()
        {
            OpenLayoutPage(TargetEditorPage);
        }

        [NaughtyAttributes.Button()]
        public void TestClosePage()
        {
            CloseLayoutPage(TargetEditorPage);
        }
#endif
    }
}