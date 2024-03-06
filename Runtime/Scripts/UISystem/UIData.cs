using UnityEngine;

namespace SeroJob.UiSystem
{
    public static class UIData
    {
        public static ProtectedAction<UIWindow> OnWindowOpened {  get; private set; }
        public static ProtectedAction<UIWindow> OnWindowClosed {  get; private set; }

        static UIData()
        {
            OnWindowClosed = new ProtectedAction<UIWindow>();
            OnWindowOpened = new ProtectedAction<UIWindow>();
        }
    }
}