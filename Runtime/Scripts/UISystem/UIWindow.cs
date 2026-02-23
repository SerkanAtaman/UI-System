using System;
using UnityEngine;
using UnityEngine.UI;

namespace SeroJob.UiSystem
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWindow : MonoBehaviour, IFlowProvider, IScaleableWindow
    {
        [SerializeField] private string _windowID;
        [SerializeField] private FlowDatabase _flowDatabase;

        [SerializeField] protected UIWindowState windowState = UIWindowState.Opened;
        [SerializeField] protected FlowController currentFlowController = null;
        [SerializeField] protected UIPage[] pages;
        [SerializeField] protected bool isScalable = false;
        [SerializeField] protected ScalableWindowElement[] scalableElements;

        public bool PreventBeingHidden = false;
        public bool DeactivateOnClose = true;
        public bool DisableCanvasOnClose = true;

        #region Property Getters

        public UIWindowState State => windowState;
        public FlowController CurrentFlowController => currentFlowController;
        public string ID => _windowID;
        public FlowDatabase FlowDatabase => _flowDatabase;
        public UIPage[] Pages => pages;
        
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

        public virtual bool IsVisible
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

        #region Variables

        protected int remainingPagesToAnimate;
        protected Action onWindowAnimatedCallback;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        internal bool arePagesHidden = false;

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            GraphicRaycaster = null;
            windowState = UIWindowState.Opened;
            if (transform.TryGetComponent(out GraphicRaycaster raycaster))
                GraphicRaycaster = raycaster;

            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowAwaken(this);
                }
            }
        }

        #endregion

        #region Public Methods

        public virtual void Open(Action callback = null)
        {
            if (State == UIWindowState.Opened || State == UIWindowState.Opening) return;

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Canvas.enabled = true;
            windowState = UIWindowState.Opening;
            onWindowAnimatedCallback = callback;

            WindowOpenStarted();
            UIData.OnWindowOpened.Invoke(this);

            if (pages == null || pages.Length < 1)
            {
                remainingPagesToAnimate = 1;
                OnPageOpened();
                return;
            }

            remainingPagesToAnimate = pages.Length;

            foreach (var page in pages)
            {
                if (page == null)
                {
                    OnPageOpened();
                    continue;
                }

                page.Open(OnPageOpened);
            }
        }

        public virtual void OpenWithoutCallback()
        {
            Open(null);
        }

        public virtual void Close(Action callback = null)
        {
            if (State == UIWindowState.Closed || State == UIWindowState.Closing) return;

            windowState = UIWindowState.Closing;

            WindowCloseStarted();
            UIData.OnWindowClosed.Invoke(this);

            onWindowAnimatedCallback = callback;

            if (pages == null || pages.Length < 1)
            {
                remainingPagesToAnimate = 1;
                OnPageClosed();
                return;
            }

            remainingPagesToAnimate = pages.Length;
            
            foreach (var page in pages)
            {
                if (page == null)
                {
                    OnPageClosed();
                    continue;
                }

                page.Close(OnPageClosed);
            }
        }

        public virtual void CloseWithoutCallback()
        {
            Close(null);
        }

        public virtual void OpenImmediately()
        {
            windowState = UIWindowState.Opening;

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Canvas.enabled = true;

            remainingPagesToAnimate = pages.Length;
            onWindowAnimatedCallback = null;
            
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
            
            remainingPagesToAnimate = pages.Length;
            onWindowAnimatedCallback = null;

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

        public virtual void SetScale(float scale)
        {
            if (scalableElements == null) return;

            foreach (var element in scalableElements)
            {
                element?.ApplyScale(scale);
            }
        }

        public T GetPage<T>() where T : UIPage
        {
            if (pages == null) return null;

            foreach (var page in pages)
            {
                if (page == null) continue;

                if (page.GetType() == typeof(T)) return (T)page;
            }

            return null;
        }

        #endregion

        #region ProtectedMethods

        protected virtual void WindowOpenStarted()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowOpenStarted(this);
                }
            }
        }
        
        protected virtual void WindowOpenEnded()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowOpenEnded(this);
                }
            }
        }
        
        protected virtual void WindowCloseStarted()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowCloseStarted(this);
                }
            }
        }
        
        protected virtual void WindowCloseEnded()
        {
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page) page.OnWindowCloseEnded(this);
                }
            }
        }

        #endregion

        #region InternalMethods
        internal void SetCurrentFlowController(FlowController flowController)
        {
            currentFlowController = flowController;
        }
        #endregion

        #region Callbacks

        private void OnPageOpened()
        {
            remainingPagesToAnimate--;

            if(remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Opened;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = true;
                WindowOpenEnded();

                var callback = onWindowAnimatedCallback;
                onWindowAnimatedCallback = null;
                remainingPagesToAnimate = -99;
                callback?.Invoke();
            }
        }

        private void OnPageClosed()
        {
            remainingPagesToAnimate--;

            if (remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Closed;
                
                WindowCloseEnded();

                if (DeactivateOnClose) gameObject.SetActive(false);
                if (DisableCanvasOnClose) Canvas.enabled = false;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;

                var callback = onWindowAnimatedCallback;
                onWindowAnimatedCallback = null;
                remainingPagesToAnimate = -99;
                callback?.Invoke();
            }
        }

        private void OnPageHiddenInstantly()
        {
            remainingPagesToAnimate--;

            if (remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Closed;

                WindowCloseEnded();

                if (DeactivateOnClose) gameObject.SetActive(false);
                if (DisableCanvasOnClose) Canvas.enabled = false;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = false;

                var callback = onWindowAnimatedCallback;
                onWindowAnimatedCallback = null;
                remainingPagesToAnimate = -99;
                callback?.Invoke();
            }
        }

        private void OnPageOpenedInstantly()
        {
            remainingPagesToAnimate--;

            if (remainingPagesToAnimate == 0)
            {
                windowState = UIWindowState.Opened;
                if (GraphicRaycaster != null) GraphicRaycaster.enabled = true;

                WindowOpenEnded();

                var callback = onWindowAnimatedCallback;
                onWindowAnimatedCallback = null;
                remainingPagesToAnimate = -99;
                callback?.Invoke();
            }
        }

        #endregion
    }
}
