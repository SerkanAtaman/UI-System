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

        public virtual void Open(Action callback)
        {
            if (PageState != UIPageState.Closed) return;

            _pageActionCallback = callback;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            StartOpening();
        }

        public virtual void Open()
        {
            if (PageState != UIPageState.Closed) return;

            _pageActionCallback = null;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            StartOpening();
        }

        public virtual void Close(Action callback)
        {
            if (PageState != UIPageState.Opened) return;

            _pageActionCallback = callback;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            StartClosing();
        }

        public virtual void Close()
        {
            if (PageState != UIPageState.Opened) return;

            _pageActionCallback = null;
            _remainingElementsToAnimate = elements == null ? 0 : elements.Length;

            StartClosing();
        }

        public virtual void OpenImmediately(Action callback)
        {
            _onStartedOpen?.Invoke();
            pageState = UIPageState.Opened;

            _openAnim.Play(callback);
            _openAnim.Complete(true);

            foreach (var element in elements)
            {
                element.OpenImmediately();
            }

            OnOpenedImmediately();
            _onFinishedOpen?.Invoke();
        }

        public virtual void OpenImmediately()
        {
            _onStartedOpen?.Invoke();
            pageState = UIPageState.Opened;

            _openAnim.Play();
            _openAnim.Complete(true);

            foreach (var element in elements)
            {
                element.OpenImmediately();
            }

            OnOpenedImmediately();
            _onFinishedOpen?.Invoke();
        }

        public virtual void HideImmediately(Action callback)
        {
            _onStartedClose?.Invoke();
            pageState = UIPageState.Closed;

            _closeAnim.Play(callback);
            _closeAnim.Complete(true);

            foreach (var element in elements)
            {
                element.HideImmediately();
            }

            OnHiddenImmediately();
            _onFinishedClose?.Invoke();
        }

        public virtual void HideImmediately()
        {
            _onStartedClose?.Invoke();
            pageState = UIPageState.Closed;

            _closeAnim.Play();
            _closeAnim.Complete(true);

            foreach (var element in elements)
            {
                element.HideImmediately();
            }

            OnHiddenImmediately();
            _onFinishedClose?.Invoke();
        }

        #endregion

        #region Protected Methods

        protected virtual void StartOpening()
        {
            pageState = UIPageState.Opening;

            _openAnim.Play(OnPageOpenAnimEnded);

            foreach (var element in elements)
            {
                element.OnOpened.AddListener(OnElementOpened);
                element.PageStartedOpening();
            }

            _onStartedOpen?.Invoke();
        }

        protected virtual void FinishOpening()
        {
            pageState = UIPageState.Opened;

            _pageActionCallback?.Invoke();
            _pageActionCallback = null;

            _onFinishedOpen?.Invoke();
        }

        protected virtual void StartClosing()
        {
            pageState = UIPageState.Closing;

            foreach (var element in elements)
            {
                element.OnClosed.AddListener(OnElementClosed);
                element.PageStartedClosing();
            }

            if (_remainingElementsToAnimate == 0) _closeAnim.Play(OnPageCloseAnimEnded);

            _onStartedClose?.Invoke();
        }

        protected virtual void FinishClosing()
        {
            pageState = UIPageState.Closed;

            _pageActionCallback?.Invoke();
            _pageActionCallback = null;

            _onFinishedClose?.Invoke();
        }

        protected virtual void OnOpenedImmediately() { }
        protected virtual void OnHiddenImmediately() { }

        protected void SetPageState(UIPageState state)
        {
            pageState = state;
        }

        #endregion

        #region Callbacks

        private void OnElementOpened(UIElement element)
        {
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
            element.OnClosed.RemoveListener(OnElementClosed);

            _remainingElementsToAnimate--;

            if (_remainingElementsToAnimate == 0)
            {
                _closeAnim.Play(OnPageCloseAnimEnded);
            }
        }

        private void OnPageOpenAnimEnded()
        {
            foreach (var element in elements)
            {
                element.PageFinishedOpening();
            }

            if (_remainingElementsToAnimate > 0) return;

            FinishOpening();
        }

        private void OnPageCloseAnimEnded()
        {
            foreach (var element in elements)
            {
                element.PageFinishedClosing();
            }

            FinishClosing();
        }

        #endregion
    }
}