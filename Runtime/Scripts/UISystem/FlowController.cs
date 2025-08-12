using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeroJob.UiSystem
{
    [DefaultExecutionOrder(-1)]
    public class FlowController : MonoBehaviour, IFlowProvider
    {
        [SerializeField] private string _flowName;
        [SerializeField] private FlowDatabase _flowDatabase;
        [SerializeField] private bool _setInitialsOnEnable = true;
        [SerializeField] private bool _closeAllOnEnable = false;

        [SerializeField] private UIWindow[] _windows;

        [SerializeField] UIWindowReference[] _initialWindows;

        [SerializeField][Foldout("Debug")] private List<UIWindow> _openedWindows;

        public Dictionary<string, UIWindow> WindowsCollection { get; private set; }
        public List<UIWindow> OpenedWindows => _openedWindows;
        public string FlowName => _flowName;

#if UNITY_EDITOR
        protected virtual void Awake()
        {
            SetWindowCollection();

            if (!UIData.IsInitialized)
            {
                UIData.EditorInit();
            }

            if (UIData.UISettings != null) UIData.UISettings.ApplySettings();
        }

        protected virtual void Start()
        {
            UIData.OnWindowOpened.AddListener(OnWindowOpened);
            UIData.OnWindowClosed.AddListener(OnWindowClosed);
            UIData.RegisterFlowController(this);
            _flowDatabase.OnCommandGiven.AddListener(OnCommandGiven);
            _openedWindows = new List<UIWindow>();

            if (_setInitialsOnEnable) OpenInitialWindows(true, true);
            if (_closeAllOnEnable) CloseAll(true);

            this.SetAllScalableWindowsScale(UIData.UISettings.UIScale);
        }
#else
        protected virtual async void Awake()
        {
            SetWindowCollection();

            if (!UIData.IsInitialized)
            {
                await UIData.Init();
            }

            if (UIData.UISettings != null) UIData.UISettings.ApplySettings();
        }

        protected virtual async void Start()
        {
            UIData.OnWindowOpened.AddListener(OnWindowOpened);
            UIData.OnWindowClosed.AddListener(OnWindowClosed);
            UIData.RegisterFlowController(this);
            _flowDatabase.OnCommandGiven.AddListener(OnCommandGiven);
            _openedWindows = new List<UIWindow>();

            if (_setInitialsOnEnable) OpenInitialWindows(true, true);
            if (_closeAllOnEnable) CloseAll(true);

            if (UIData.UISettings != null)
            {
                this.SetAllScalableWindowsScale(UIData.UISettings.UIScale);
            }
            else
            {
                await UIData.Init();
                this.SetAllScalableWindowsScale(UIData.UISettings.UIScale);
            }
        }
#endif

        protected virtual void OnDestroy()
        {
            UIData.OnWindowOpened.RemoveListener(OnWindowOpened);
            UIData.OnWindowClosed.RemoveListener(OnWindowClosed);
            UIData.UnregisterFlowController(this);
            _flowDatabase.OnCommandGiven.RemoveListener(OnCommandGiven);
            ClearWindowCollection();
        }

        private void SetWindowCollection()
        {
            WindowsCollection = new();

            foreach (var item in _windows)
            {
                if (item == null) continue;

                item.SetCurrentFlowController(this);
                WindowsCollection.Add(item.ID, item);
            }
        }

        private void ClearWindowCollection()
        {
            if (WindowsCollection == null) return;

            int count = WindowsCollection.Count;

            for (int i = 0; i < count; i++)
            {
                var window = WindowsCollection.ElementAt(i).Value;
                if (window != null && window.CurrentFlowController == this)
                    window.SetCurrentFlowController(null);
            }

            WindowsCollection.Clear();
            WindowsCollection = null;
        }

#if UNITY_EDITOR
        public void OpenInitialWindows(bool openImmediately, bool closeOthers = false)
        {
            if (!UIData.IsInitialized) UIData.EditorInit();

            this.SetAllWindowVisibility(!_flowDatabase.HideAllWindows);

            if (closeOthers)
            {
                foreach (var window in _windows)
                {
                    if (!WindowRefArrayContains(_initialWindows, window))
                    {
                        CloseWindow(window, openImmediately, false);
                    }
                }
            }

            foreach (var window in _windows)
            {
                if (WindowRefArrayContains(_initialWindows, window))
                {
                    if (window.State == UIWindowState.Opened && !openImmediately)
                    {
                        if (!_openedWindows.Contains(window)) _openedWindows.Add(window);
                    }
                    else
                    {
                        OpenWindow(window, openImmediately, false);
                    }
                }
            }
        }
#else
        public async void OpenInitialWindows(bool openImmediately, bool closeOthers = false)
        {
            if (!UIData.IsInitialized) await UIData.Init();

            this.SetAllWindowVisibility(!_flowDatabase.HideAllWindows);

            if (closeOthers)
            {
                foreach (var window in _windows)
                {
                    if (!WindowRefArrayContains(_initialWindows, window))
                    {
                        CloseWindow(window, openImmediately, false);
                    }
                }
            }

            foreach (var window in _windows)
            {
                if (WindowRefArrayContains(_initialWindows, window))
                {
                    if (window.State == UIWindowState.Opened && !openImmediately)
                    {
                        if (!_openedWindows.Contains(window)) _openedWindows.Add(window);
                    }
                    else
                    {
                        OpenWindow(window, openImmediately, false);
                    }
                }
            }
        }
#endif

        public void CloseAll(bool closeImmediately)
        {
            foreach (var window in _windows)
            {
                CloseWindow(window, closeImmediately, false);
            }
        }

        private void OpenWindow(UIWindow window, bool openImidiate = false, bool isTrackable = true)
        {
            if (window.State == UIWindowState.Opened) return;
            if (window.State == UIWindowState.Opening && !openImidiate) return;

            GiveCommand(new OpenWindowsCommand(new UIWindow[] { window }, openImidiate, isTrackable));
        }


        private void CloseWindow(UIWindow window, bool closeImidiate = false, bool isTrackable = true)
        {
            if (window.State == UIWindowState.Closed) return;
            if (window.State == UIWindowState.Closing && !closeImidiate) return;

            GiveCommand(new CloseWindowsCommand(new UIWindow[] { window }, closeImidiate, isTrackable));
        }

        public void OpenWindow(string windowID, Action callback = null, bool openImmediate = false, bool solveConflictsAfterOpen = false, bool solveConflictsImmediately = false, bool waitForConflicts = true)
        {
            try
            {
                var window = WindowsCollection[windowID];
                if (window.State == UIWindowState.Opened) return;
                if (window.State == UIWindowState.Opening && !openImmediate) return;
                var command = new OpenWindowsCommand(new UIWindow[] { window }, openImmediate, solveConflictsAfterOpen, solveConflictsImmediately, waitForConflicts)
                {
                    OnCompleted = callback
                };
                GiveCommand(command);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void CloseWindow(string windowID, Action callback = null, bool closeImmediate = false, bool solveConflictsAfterClose = false)
        {
            try
            {
                var window = WindowsCollection[windowID];
                if (window.State == UIWindowState.Closed) return;
                if (window.State == UIWindowState.Closing && !closeImmediate) return;
                var command = new CloseWindowsCommand(new UIWindow[] { window }, closeImmediate, solveConflictsAfterClose)
                {
                    OnCompleted = callback
                };
                GiveCommand(command);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void GiveCommand(UICommand command, bool trackable = true)
        {
            if (command == null) return;
            var process = command.GetProccess(this);
            if (process == null)
            {
                command.Dispose();
                return;
            }

            void onProccessWorked(UIProccess sender)
            {
                sender.OnWorkCompleted.RemoveListener(onProccessWorked);
                command.OnCompleted?.Invoke();
                command.Dispose();
            }

            process.OnWorkCompleted.AddListener(onProccessWorked);
            process.Work();
        }

        public FlowDatabase GetFlowDatabase()
        {
            return _flowDatabase;
        }

        public UIWindow GetWindowByID(string id)
        {
            try
            {
                return WindowsCollection[id];
            }
            catch
            {
                return null;
            }
        }

        public T GetWindowByType<T>() where T : UIWindow
        {
            foreach (var window in _windows)
            {
                if (window.GetType() == typeof(T)) return (T)window;
            }

            return null;
        }

        #region Events

        protected virtual void OnWindowOpened(UIWindow window)
        {
            _openedWindows.Add(window);
        }

        protected virtual void OnWindowClosed(UIWindow window)
        {
            _openedWindows.Remove(window);
        }

        protected virtual void OnCommandGiven(UICommand command)
        {
            GiveCommand(command);
        }

        #endregion

        #region Helper Methods

        private bool WindowRefArrayContains(UIWindowReference[] array, UIWindow window)
        {
            bool result = false;

            string windowID = window.ID;

            foreach (var item in array)
            {
                if (item.WindowID == windowID)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
        #endregion

        #region Editor Functions
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (_windows.IsNullOrEmpty()) return;
            if(_flowDatabase == null)
            {
                Debug.LogWarning("FlowDatabase reference is null! Please provide a valid FlowDatabase object", gameObject);
                return;
            }

            _flowDatabase.FlowName = _flowName;

            if (_flowDatabase.GetRawWindowIds().Length == _windows.Length) return;

            string[] windowIDs = new string[_windows.Length];

            for (int i = 0; i < windowIDs.Length; i++)
            {
                windowIDs[i] = _windows[i].ID;
            }

            _flowDatabase.WindowIDs = windowIDs;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.EditorUtility.SetDirty(_flowDatabase);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(_flowDatabase);
            };
        }

#endif
#endregion
    }
}