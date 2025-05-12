using System.Collections.Generic;
using UnityEngine;

namespace SeroJob.UiSystem
{
    public static class UIData
    {
        public static ProtectedAction<UIWindow> OnWindowOpened {  get; private set; }
        public static ProtectedAction<UIWindow> OnWindowClosed {  get; private set; }

        public static List<FlowController> RegisteredFlowControllers { get; private set; }

        public static bool IsInitialized { get; private set; } = false;
        public static bool IsInitializing { get; private set; } = false;

        public static UISettings UISettings { get; private set; }

        static UIData()
        {
            IsInitialized = false;
            IsInitializing = false;
            UISettings = null;
            OnWindowClosed = new();
            OnWindowOpened = new();
            RegisteredFlowControllers = new();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Reset()
        {
            IsInitialized = false;
            IsInitializing = false;
            UISettings = null;
        }

        public static async System.Threading.Tasks.Task Init()
        {
            if (IsInitialized) return;

            if (IsInitializing)
            {
                while (!IsInitialized)
                {
                    await System.Threading.Tasks.Task.Delay(300);
                    return;
                }
            }

            Debug.Log("Initializing UI Data");

            IsInitializing = true;
            var op = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<UISettings>("SerojobUISystemSettings");

            while (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.None || !op.IsDone)
            {
                await System.Threading.Tasks.Task.Delay(300);
            }

            UISettings = op.Result;
            IsInitialized = true;
            IsInitializing = false;

            Debug.Log("UI Data is now initialized");
        }

        public static void RegisterFlowController(FlowController flowController)
        {
            if (!RegisteredFlowControllers.Contains(flowController)) 
                RegisteredFlowControllers.Add(flowController);
        }

        public static void UnregisterFlowController(FlowController flowController)
        {
            if (RegisteredFlowControllers.Contains(flowController))
                RegisteredFlowControllers.Remove(flowController);
        }

        public static FlowController GetRegisteredFlowControllerByName(string name)
        {
            foreach (var flowController in RegisteredFlowControllers)
            {
                if (string.Equals(name, flowController.FlowName)) return flowController;
            }

            return null;
        }
    }
}