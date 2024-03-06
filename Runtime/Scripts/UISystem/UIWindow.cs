using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using System.Collections;

namespace SeroJob.UiSystem
{
    [RequireComponent(typeof(Canvas))]
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

        public Canvas Canvas => _canvas;

        public int SortingOrder => _canvas.sortingOrder;

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

        #endregion

        #region Private Variables

        private int _remainingPagesToAnimate;

        private Canvas _canvas;

        private Action _onWindowAnimatedCallback;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            GraphicRaycaster = null;
            windowState = UIWindowState.Opened;
            _canvas = GetComponent<Canvas>();
            if (transform.TryGetComponent(out GraphicRaycaster raycaster))
                GraphicRaycaster = raycaster;
        }

        #endregion

        #region Public Methods

        public virtual void Open(Action callback = null)
        {
            if (State != UIWindowState.Closed) return;
            
            gameObject.SetActive(true);
            _canvas.enabled = true;

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
            _canvas.enabled = true;

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
            _canvas.enabled = true;

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
            _canvas.sortingOrder = order;
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
                _canvas.enabled = false;

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
                StartCoroutine(Delay(0.1f, () =>
                {
                    windowState = UIWindowState.Closed;
                    
                    WindowCloseEnded();
                    
                    gameObject.SetActive(false);
                    _canvas.enabled = false;

                    _onWindowAnimatedCallback?.Invoke();
                    _onWindowAnimatedCallback = null;
                    _remainingPagesToAnimate = -99;
                }));
            }
        }

        private void OnPageOpenedInstantly()
        {
            _remainingPagesToAnimate--;

            if (_remainingPagesToAnimate == 0)
            {
                StartCoroutine(Delay(0.1f, () =>
                {
                    windowState = UIWindowState.Opened;
                    
                    WindowOpenEnded();
                    
                    _onWindowAnimatedCallback?.Invoke();
                    _onWindowAnimatedCallback = null;
                    _remainingPagesToAnimate = -99;
                }));
            }
        }

        #endregion

        #region Helper Methods

        private IEnumerator Delay(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);

            callback();
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
            _canvas = GetComponent<Canvas>();
            HideImmediately();
        }

        [ContextMenu("Open")]
        private void OpenFromEditor()
        {
            _canvas = GetComponent<Canvas>();
            OpenImmediately();
        }

#endif
        #endregion
    }
}
