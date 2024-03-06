using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public static class UIHelper
    {
        public static List<UIWindow> GetConflictedWindows(UIWindow refWindow, List<UIWindow> openedWindows)
        {
            List<UIWindow> result = new List<UIWindow>();

            if (refWindow.CooperatedWindows.Contains("everything"))
                return result;

            foreach (var window in openedWindows)
            {
                if(window.CooperatedWindows.Contains("everything")) continue;
                
                if (!refWindow.CooperatedWindows.Contains(window.ID))
                {
                    result.Add(window);
                }
            }

            return result;
        }

        public static List<UIWindow> GetConflictedWindows(UIWindow[] refWindows, List<UIWindow> openedWindows)
        {
            List<UIWindow> result = new List<UIWindow>();

            foreach (var window in openedWindows)
            {
                foreach (var window2 in refWindows)
                {
                    if(window2.CooperatedWindows.Contains("everything")) continue;
                    
                    if (!window2.CooperatedWindows.Contains(window.ID))
                    {
                        if(!result.Contains(window)) result.Add(window);
                    }
                }
            }

            return result;
        }

        public static bool DoesWindowsConflict(UIWindow[] windows)
        {
            bool result = false;

            for(int i = 0; i < windows.Length - 1; i++)
            {
                if (!windows[i] == windows[i + 1])
                {
                    continue;
                }

                if (!windows[i].CooperatedWindows.Contains(windows[i + 1].ID))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static UIWindow[] GetOpenableWindows(UIWindow[] windows)
        {
            List<UIWindow> list = new List<UIWindow>();

            foreach (UIWindow window in windows)
            {
                if (window.State == UIWindowState.Closed) list.Add(window);
            }

            return list.ToArray();
        }

        public static UIWindow[] GetClosableWindows(UIWindow[] windows)
        {
            List<UIWindow> list = new List<UIWindow>();

            foreach (UIWindow window in windows)
            {
                if (window.State == UIWindowState.Opened) list.Add(window);
            }

            return list.ToArray();
        }

        public static UIWindow FindWindowIn(UIWindow[] array, string id)
        {
            UIWindow result = null;

            foreach (UIWindow window in array)
            {
                if(window.ID == id)
                {
                    result = window;
                    break;
                }
            }

            return result;
        }

        public static UIWindowReference[] ConvertToWindowReferenceArray(UIWindow[] windows)
        {
            if (windows.IsNullOrEmpty())
            {
                return new UIWindowReference[0];
            }

            int count = windows.Length;
            UIWindowReference[] array = new UIWindowReference[count];

            for(int i = 0; i < count; i++)
            {
                array[i] = new UIWindowReference(windows[i].ID);
            }

            return array;
        }
    }
}