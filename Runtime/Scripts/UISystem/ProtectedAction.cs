using System;

namespace SeroJob.UiSystem
{
    public class ProtectedAction<T1>
    {
        private Action<T1> _action;

        public ProtectedAction()
        {
            _action = null;
        }

        public void AddListener(Action<T1> listener)
        {
            _action += listener;
        }

        public void RemoveListener(Action<T1> listener)
        {
            _action -= listener;
        }

        public void Invoke(T1 t1)
        {
            _action?.Invoke(t1);
        }
    }
}