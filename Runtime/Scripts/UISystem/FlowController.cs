using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeroJob.UiSystem
{
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

        private bool InputReceivable
        {
            set
            {
                foreach (var item in _windows)
                {
                    if (item.GraphicRaycaster) item.GraphicRaycaster.enabled = value;
                }
            }
        }

        protected virtual void OnEnable()
        {
            UIProccessTracker.Reset();

            if (UIResourceHelper.TryLoadSettings())
            {
                UIResourceHelper.Settings.ApplySettings();
                UIResourceHelper.ReleaseSettings();
            }

            UIData.OnWindowOpened.AddListener(OnWindowOpened);
            UIData.OnWindowClosed.AddListener(OnWindowClosed);
            UIData.RegisterFlowController(this);
            _flowDatabase.OnCommandGiven.AddListener(OnCommandGiven);

            _openedWindows = new List<UIWindow>();
            SetWindowCollection();

            if (_setInitialsOnEnable) OpenInitialWindows(true, true);
            if (_closeAllOnEnable) CloseAll(true);
        } 

        protected virtual void OnDisable()
        {
            UIData.OnWindowOpened.RemoveListener(OnWindowOpened);
            UIData.OnWindowClosed.RemoveListener(OnWindowClosed);
            UIData.UnregisterFlowController(this);
            _flowDatabase.OnCommandGiven.RemoveListener(OnCommandGiven);
        }

        private void SetWindowCollection()
        {
            WindowsCollection = new();

            foreach (var item in _windows)
            {
                WindowsCollection.Add(item.ID, item);
            }
        }

        public void OpenInitialWindows(bool openImmediately, bool closeOthers = false)
        {
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
                    if (window.State == UIWindowState.Opened) _openedWindows.Add(window);
                    else OpenWindow(window, openImmediately, false);
                }
            }
        }

        public void CloseAll(bool closeImmediately)
        {
            foreach (var window in _windows)
            {
                CloseWindow(window, closeImmediately, false);
            }
        }

        private void OpenWindow(UIWindow window, bool openImidiate = false, bool isTrackable = true)
        {
            if (window.State != UIWindowState.Closed && !openImidiate) return;

            GiveCommand(new OpenWindowsCommand(new UIWindow[] { window }, openImidiate, isTrackable));
        }


        private void CloseWindow(UIWindow window, bool closeImidiate = false, bool isTrackable = true)
        {
            if (window.State != UIWindowState.Opened && !closeImidiate) return;

            GiveCommand(new CloseWindowsCommand(new UIWindow[] { window }, closeImidiate, isTrackable));
        }

        public void OpenWindow(string windowID, Action callback = null, bool openImmediate = false, bool solveConflictsAfterOpen = false, bool solveConflictsImmediately = false, bool waitForConflicts = true)
        {
            try
            {
                var window = WindowsCollection[windowID];
                if (window.State != UIWindowState.Closed && !openImmediate) return;
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
                if (window.State != UIWindowState.Opened && !closeImmediate) return;
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