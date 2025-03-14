using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

namespace SeroJob.UiSystem
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindow : MonoBehaviour, IFlowProvider
    {
        [ReadOnly][SerializeField] protected UIWindowState windowState = UIWindowState.Opened;

        [field:SerializeField] public string ID { get; private set; }
        [field:SerializeField] public FlowDatabase FlowDatabase { get; private set; }

        [SerializeField] [OnValueChanged("OnCollabratorWindowsChanged")] UIWindowReference[] _collaboratorWindows;

        [SerializeField] protected UIPage[] pages;
        
        #region Property Getters

        public UIWindowState State => windowState;
        
        public GraphicRaycaster GraphicRaycaster { get; private set; }

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null) _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        public int SortingOrder => Canvas.sortingOrder;

        public string[] CooperatedWindows
        {
            get
            {
                if (_collaboratorWindows == null || _collaboratorWindows.Length == 0) return null;

                string[] array = new string[_collaboratorWindows.Length];

                for(int i = 0;  i < _collaboratorWindows.Length; i++)
                {
                    array[i] = _collaboratorWindows[i].WindowID;
                }

                return array;
            }
        }
        public bool IsVisible
        {
            get
            {
                return CanvasGroup.alpha > 0.02f;
            }
            set
            {
                CanvasGroup.alpha = value ? 1f : 0f;
            }
        }

        #endregion

        #region Private Variables

        private int _remainingPagesToAnimate;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private Action _onWindowAnimatedCallback;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            GraphicRaycaster = null;
            windowState = UIWindowState.Opened;
            if (transform.TryGetComponent(out GraphicRaycaster raycaster))
                GraphicRaycaster = raycaster;
        }

        #endregion

        #region Public Methods

        public virtual void Open(Action callback = null)
        {
            if (State != UIWindowState.Closed) return;
            
            gameObject.SetActive(true);
            Canvas.enabled = true;

            windowState = UIWindowState.Opening;

            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = callback;
            
            WindowOpenStarted();
            UIData.OnWindowOpened.Invoke(this);

            foreach (var page in pages)
            {
                page.Open(OnPageOpened);
            }
        }

        public virtual void OpenWithoutCallback()
        {
            if (State != UIWindowState.Closed) return;

            gameObject.SetActive(true);
            Canvas.enabled = true;

            windowState = UIWindowState.Opening;

            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = null;

            WindowOpenStarted();
            UIData.OnWindowOpened.Invoke(this);

            foreach (var page in pages)
            {
                page.Open(OnPageOpened);
            }
        }

        public virtual void Close(Action callback = null)
        {
            if (State != UIWindowState.Opened) return;
            
            windowState = UIWindowState.Closing;

            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = callback;
            
            WindowCloseStarted();
            UIData.OnWindowClosed.Invoke(this);

            foreach (var page in pages)
            {
                page.Close(OnPageClosed);
            }
        }

        public virtual void CloseWithoutCallback()
        {
            if (State != UIWindowState.Opened) return;

            windowState = UIWindowState.Closing;

            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = null;

            WindowCloseStarted();
            UIData.OnWindowClosed.Invoke(this);

            foreach (var page in pages)
            {
                page.Close(OnPageClosed);
            }
        }

        public virtual void OpenImmediately()
        {
            windowState = UIWindowState.Opening;
            
            gameObject.SetActive(true);
            Canvas.enabled = true;

            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = null;
            
            WindowOpenStarted();

            foreach (var page in pages)
            {
                page.OpenImmediately(OnPageOpenedInstantly);
            }

            UIData.OnWindowOpened.Invoke(this);
        }

        public virtual void HideImmediately()
        {
            windowState = UIWindowState.Closing;
            
            _remainingPagesToAnimate = pages.Length;
            _onWindowAnimatedCallback = null;

            WindowCloseStarted();
            
            foreach (var page in pages)
            {
                page.HideImmediately(OnPageHiddenInstantly);
            }

            UIData.OnWindowClosed.Invoke(this);
        }

        public void SetSortingOrder(int order)
        {
            Canvas.sortingOrder = order;
        }

        public FlowDatabase GetFlowDatabase()
        {
            return FlowDatabase;
        }

        #endregion

        #region ProtectedMethods

        protected virtual void WindowOpenStarted()
        {
            
        }
        
        protected virtual void WindowOpenEnded()
        {
            
        }
        
        protected virtual void WindowCloseStarted()
        {
            
        }
        
        protected virtual void WindowCloseEnded()
        {
            
        }

        #endregion

        #region Callbacks

        private void OnPageOpened()
        {
            _remainingPagesToAnimate--;

            if(_remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Opened;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = true;
                WindowOpenEnded();
                
                _onWindowAnimatedCallback?.Invoke();
                _onWindowAnimatedCallback = null;
                _remainingPagesToAnimate = -99;
            }
        }

        private void OnPageClosed()
        {
            _remainingPagesToAnimate--;

            if (_remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Closed;
                
                WindowCloseEnded();
                
                gameObject.SetActive(false);
                Canvas.enabled = false;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;

                _onWindowAnimatedCallback?.Invoke();
                _onWindowAnimatedCallback = null;
                _remainingPagesToAnimate = -99;
            }
        }

        private void OnPageHiddenInstantly()
        {
            _remainingPagesToAnimate--;

            if (_remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Closed;

                WindowCloseEnded();

                gameObject.SetActive(false);
                Canvas.enabled = false;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;

                _onWindowAnimatedCallback?.Invoke();
                _onWindowAnimatedCallback = null;
                _remainingPagesToAnimate = -99;
            }
        }

        private void OnPageOpenedInstantly()
        {
            _remainingPagesToAnimate--;

            if (_remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Opened;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = true;

                WindowOpenEnded();

                _onWindowAnimatedCallback?.Invoke();
                _onWindowAnimatedCallback = null;
                _remainingPagesToAnimate = -99;
            }
        }

        #endregion

        #region Editor Methods
#if UNITY_EDITOR

        private void OnCollabratorWindowsChanged()
        {
            UnityEditor.EditorApplication.delayCall += CheckNewCollabrators;
        }

        private void CheckNewCollabrators()
        {
            var collabrators = CooperatedWindows;
            if (collabrators.Contains(ID))
            {
                UIWindowReference item = null;

                foreach (var window in _collaboratorWindows)
                {
                    if (window.WindowID == ID)
                    {
                        item = window;
                        break;
                    }
                }

                _collaboratorWindows = _collaboratorWindows.Remove(item);
            }
        }

        [ContextMenu("Close")]
        private void HideFromEditor()
        {
            if (!Application.isPlaying)
            {
                gameObject.SetActive(false);
                Canvas.enabled = false;
                windowState = UIWindowState.Closed;
                return;
            }

            HideImmediately();
        }

        [ContextMenu("Open")]
        private void OpenFromEditor()
        {
            if (!Application.isPlaying)
            {
                gameObject.SetActive(true);
                Canvas.enabled = true;
                windowState = UIWindowState.Opened;
                return;
            }

            OpenImmediately();
        }

#endif
        #endregion
    }
}
