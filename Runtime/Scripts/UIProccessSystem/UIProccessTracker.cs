using System;
using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public static class UIProccessTracker
    {
        private static Stack<UIProccess> _proccessStack;

        private static Action _callback = null;

        static UIProccessTracker()
        {
            _proccessStack = new Stack<UIProccess>();
        }

        public static void Reset()
        {
            _proccessStack.Clear();
            _proccessStack = new Stack<UIProccess>();
        }

        public static void ExpandTrack(UIProccess proccess)
        {
            _proccessStack.Push(proccess);
        }

        public static void Reverse(Action onEnd = null)
        {
            if (_proccessStack.Count == 0)
            {
                onEnd?.Invoke();
                return;
            }

            var proccess = _proccessStack.Pop();

            _callback = onEnd;

            proccess.OnReworkCompleted.AddListener(OnReverseCompleted);

            proccess.Rework();
        }

        private static void OnReverseCompleted(UIProccess sender)
        {
            sender.OnReworkCompleted.RemoveListener(OnReverseCompleted);

            _callback?.Invoke();
            _callback = null;
        }
    }
}