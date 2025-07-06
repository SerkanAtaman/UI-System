using System.Collections.Generic;
using System.Linq;

namespace SeroJob.UiSystem
{
    public static class UIHelper
    {
        public static List<UIWindow> GetConflictedWindows(UIWindow refWindow, List<UIWindow> openedWindows)
        {
            var result = new List<UIWindow>();

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
            var result = new List<UIWindow>();

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

        public static void SetWindowVisibility(this FlowController flowController, bool isVisible, params string[] windowIDs)
        {
            if (flowController == null || windowIDs == null) return;

            foreach (string windowID in windowIDs)
            {
                var window = flowController.GetWindowByID(windowID);
                if (window == null) continue;
                if (window.PreventBeingHidden && !isVisible) continue;

                window.IsVisible = isVisible;
            }
        }

        public static void SetAllWindowVisibility(this FlowController flowController, bool isVisible)
        {
            if (flowController == null) return;

            int count = flowController.WindowsCollection.Count;

            for (int i = 0; i < count; i++)
            {
                var window = flowController.WindowsCollection.ElementAt(i).Value;
                if (window == null) continue;
                if (window.PreventBeingHidden && !isVisible) continue;

                window.IsVisible = isVisible;
            }
        }

        public static void SetAllScalableWindowsScale(this FlowController flowController, float scale)
        {
            if (flowController == null || flowController.WindowsCollection == null) return;

            int count = flowController.WindowsCollection.Count;

            for (int i = 0; i < count; i++)
            {
                var window = flowController.WindowsCollection.ElementAt(i).Value;
                if (window == null) continue;
                if (window is IScaleableWindow scaleable) scaleable.SetScale(scale);
            }
        }
    }
}