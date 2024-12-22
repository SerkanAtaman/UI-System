using System.Collections.Generic;

namespace SeroJob.UiSystem
{
    public static class UIData
    {
        public static ProtectedAction<UIWindow> OnWindowOpened {  get; private set; }
        public static ProtectedAction<UIWindow> OnWindowClosed {  get; private set; }

        public static List<FlowController> RegisteredFlowControllers { get; private set; }

        static UIData()
        {
            OnWindowClosed = new();
            OnWindowOpened = new();
            RegisteredFlowControllers = new();
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