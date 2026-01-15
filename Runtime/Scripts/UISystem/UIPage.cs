using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using System;
using BrunoMikoski.AnimationSequencer;

namespace SeroJob.UiSystem
{
    [RequireComponent(typeof(RectTransform))]
    public class UIPage : MonoBehaviour
    {
        [ReadOnly][SerializeField] protected UIPageState pageState = UIPageState.Opened;

        [SerializeField] protected UIElement[] elements;

        #region Animations

        [SerializeField] [Foldout("Animations")] private AnimationSequencerController _openAnim;
        [SerializeField] [Foldout("Animations")] private AnimationSequencerController _closeAnim;

        public AnimationSequencerController OpenAnim => _openAnim;
        public AnimationSequencerController CloseAnim => _closeAnim;

        #endregion

        #region Events
        [SerializeField] [Foldout("Events")] private UnityEvent _onStartedOpen;
        [SerializeField] [Foldout("Events")] private UnityEvent _onFinishedOpen;
        [SerializeField] [Foldout("Events")] private UnityEvent _onStartedClose;
        [SerializeField] [Foldout("Events")] private UnityEvent _onFinishedClose;

        #endregion

        #region PropertyGetters

        public UIPageState PageState => pageState;
        public UnityEvent OnStartedOpen => _onStartedOpen;
        public UnityEvent OnFinishedOpen => _onFinishedOpen;
        public UnityEvent OnStartedClose => _onStartedClose;
        public UnityEvent OnFinishedClose => _onFinishedClose;

        #endregion

        private Action _pageActionCallback;

        private int _remainingElementsToAnimate;

        #region Public Methos

        public virtual void Open(Action callback, bool force = false)
        {
            if (PageState == UIPageState.Opening || PageState == UIPageState.Opened)
            {
                if (!force)
                {
                    callback?.Invoke();
                    return;
                }
            }

            _pageActionCallback = callback;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            OnOpenStarted();
            StartOpening();
        }

        public void Open()
        {
            Open(null);
        }

        public virtual void Close(Action callback)
        {
            if (PageState == UIPageState.Closing || pageState == UIPageState.Closed)
            {
                callback?.Invoke();
                return;
            }

            _pageActionCallback = callback;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            OnHideStarted();
            StartClosing();
        }

        public virtual void Close()
        {
            Close(null);
        }

        public virtual void OpenImmediately(Action callback)
        {
            _openAnim.Kill(false);
            _closeAnim.Kill(false);

            pageState = UIPageState.Opening;
            OnOpenStarted();
            _onStartedOpen?.Invoke();

            _openAnim.Play();
            _openAnim.Complete(true);

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null) continue;
                    element.OpenImmediately();
                }
            }

            pageState = UIPageState.Opened;
            OnHideEnded();
            _onFinishedOpen?.Invoke();
            callback?.Invoke();
        }

        public virtual void OpenImmediately()
        {
            OpenImmediately(null);
        }

        public virtual void HideImmediately(Action callback)
        {
            _closeAnim.Kill(false);
            _openAnim.Kill(false);

            pageState = UIPageState.Closing;
            OnHideStarted();
            _onStartedClose?.Invoke();

            _closeAnim.Play();
            _closeAnim.Complete(true);

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null) continue;
                    element.HideImmediately();
                }
            }

            pageState = UIPageState.Closed;
            OnHideEnded();
            _onFinishedClose?.Invoke();
            callback?.Invoke();
        }

        public virtual void HideImmediately()
        {
            HideImmediately(null);
        }

        #endregion

        #region Protected Methods

        protected virtual void StartOpening()
        {
            pageState = UIPageState.Opening;

            _openAnim.Kill(false);
            _closeAnim.Kill(false);
            _openAnim.Play(OnPageOpenAnimEnded);

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null) continue;
                    element.OnOpened.AddListener(OnElementOpened);
                    element.PageStartedOpening();
                }
            }

            _onStartedOpen?.Invoke();
        }

        protected virtual void FinishOpening()
        {
            pageState = UIPageState.Opened;

            var callback = _pageActionCallback;
            _pageActionCallback = null;
            callback?.Invoke();

            _onFinishedOpen?.Invoke();
        }

        protected virtual void StartClosing()
        {
            pageState = UIPageState.Closing;

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null)
                    {
                        OnElementClosed(null);
                        continue;
                    }

                    element.OnClosed.AddListener(OnElementClosed);
                    element.PageStartedClosing();
                }
            }

            if (_remainingElementsToAnimate == 0)
            {
                _closeAnim.Kill(false);
                _openAnim.Kill(false);
                _closeAnim.Play(OnPageCloseAnimEnded);
            }

            _onStartedClose?.Invoke();
        }

        protected virtual void FinishClosing()
        {
            pageState = UIPageState.Closed;

            var callback = _pageActionCallback;
            _pageActionCallback = null;
            callback?.Invoke();

            _onFinishedClose?.Invoke();
        }

        protected virtual void OnOpenStarted() { }
        protected virtual void OnOpenEnded() { }
        protected virtual void OnHideStarted() { }
        protected virtual void OnHideEnded() { }
        internal protected virtual void OnWindowAwaken(UIWindow window) { }
        internal protected virtual void OnWindowOpenStarted(UIWindow window) { }
        internal protected virtual void OnWindowOpenEnded(UIWindow window) { }
        internal protected virtual void OnWindowCloseStarted(UIWindow window) { }
        internal protected virtual void OnWindowCloseEnded(UIWindow window) { }

        protected void SetPageState(UIPageState state)
        {
            pageState = state;
        }

        #endregion

        #region Callbacks

        private void OnElementOpened(UIElement element)
        {
            if (element)
                element.OnOpened.RemoveListener(OnElementOpened);

            _remainingElementsToAnimate--;

            if (_remainingElementsToAnimate == 0)
            {
                if (_openAnim.IsPlaying) return;

                FinishOpening();
            }
        }

        private void OnElementClosed(UIElement element)
        {
            if (element)
                element.OnClosed.RemoveListener(OnElementClosed);

            _remainingElementsToAnimate--;

            if (_remainingElementsToAnimate == 0)
            {
                _closeAnim.Kill(false);
                _closeAnim.Play(OnPageCloseAnimEnded);
            }
        }

        private void OnPageOpenAnimEnded()
        {
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null) continue;
                    element.PageFinishedOpening();
                }
            }

            if (_remainingElementsToAnimate > 0) return;

            FinishOpening();
        }

        private void OnPageCloseAnimEnded()
        {
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    if (element == null) continue;
                    element.PageFinishedClosing();
                }
            }

            FinishClosing();
        }

        #endregion
    }
}