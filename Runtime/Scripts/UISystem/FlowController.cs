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

        [SerializeField] private UIWindow[] _windows;

        [SerializeField] UIWindowReference[] _initialWindows;

        [SerializeField][Foldout("Debug")] private List<UIWindow> _openedWindows;

        [SerializeField][Foldout("Debug")] private bool _isBusy;
        [SerializeField][Foldout("Debug")] private int _totalWindowProccessCount;
        [SerializeField][Foldout("Debug")] private int _totalConflictProccessCount;

        public Dictionary<string, UIWindow> WindowsCollection { get; private set; }
        public List<UIWindow> OpenedWindows => _openedWindows;
        public bool IsBusy => _isBusy;
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

        private readonly Queue<UICommand> _commandQueue = new();

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

            _isBusy = false;

            SetInitialWindows();
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

        private void SetInitialWindows()
        {
            foreach (var window in _windows)
            {
                if (!WindowRefArrayContains(_initialWindows, window))
                {
                    CloseWindow(window, true, false);
                }
            }

            foreach (var window in _windows)
            {
                if (WindowRefArrayContains(_initialWindows, window))
                {
                    if (window.State == UIWindowState.Opened) _openedWindows.Add(window);
                    else OpenWindow(window, true, false);
                }
            }
        }

        private void OpenWindow(UIWindow window, bool openImidiate = false, bool isTrackable = true)
        {
            if (window.State != UIWindowState.Closed) return;

            GiveCommand(new OpenWindowsCommand(new UIWindow[] { window }, openImidiate, isTrackable));
        }


        private void CloseWindow(UIWindow window, bool closeImidiate = false, bool isTrackable = true)
        {
            if (window.State != UIWindowState.Opened) return;

            GiveCommand(new CloseWindowsCommand(new UIWindow[] { window }, closeImidiate, isTrackable));
        }

        private void WorkOnCommandQueue()
        {
            if(_commandQueue.Count == 0)
            {
                _isBusy = false;
                InputReceivable = true;
                return;
            }

            var command = _commandQueue.Dequeue();
            var process = command.GetProccess(this);

            if(process == null)
            {
                command.Dispose();
                WorkOnCommandQueue();
                return;
            }

            void onProccessWorked(UIProccess sender)
            {
                sender.OnWorkCompleted.RemoveListener(onProccessWorked);
                command.OnCompleted?.Invoke();
                command.Dispose();
                WorkOnCommandQueue();
            }

            process.OnWorkCompleted.AddListener(onProccessWorked);
            process.Work();
        }

        private void WorkOnCommandWithoutQeuing(UICommand command)
        {
            var process = command.GetProccess(this);
            if (process == null) return;

            void onProccessWorked(UIProccess sender)
            {
                sender.OnWorkCompleted.RemoveListener(onProccessWorked);
                command.OnCompleted?.Invoke();
                command.Dispose();
            }

            process.OnWorkCompleted.AddListener(onProccessWorked);
            process.Work();
        }

        public void GoBack()
        {
            if (_isBusy) return;

            _isBusy = true;

            UIProccessTracker.Reverse(() => _isBusy = false);
        }

        public void OpenWindow(string windowID, Action callback = null, bool openImmediate = false, bool solveConflictsAfterOpen = false, bool solveConflictsImmediately = false, bool waitForConflicts = true)
        {
            try
            {
                var window = WindowsCollection[windowID];
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
            if (_isBusy)
            {
                if (!command.QueueCommand)
                {
                    WorkOnCommandWithoutQeuing(command);
                }
                else
                {
                    _commandQueue.Enqueue(command);
                }
                
                return;
            }

            _isBusy = true;
            InputReceivable = false;

            _commandQueue.Enqueue(command);
            WorkOnCommandQueue();
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