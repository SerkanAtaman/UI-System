using UnityEngine;

namespace SeroJob.UiSystem
{
    public static class UIDebugger
    {
        public static bool DebugEnabled = true;

        public enum DebugColor
        {
            Black,
            Blue,
            Cyan,
            Gray,
            Green,
            Grey,
            Magenta,
            Red,
            White,
            Yellow
        }

        public static void LogMessage(string message)
        {
            if (!DebugEnabled) return;

            LogMessage(message, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogMessage(string action, string additionalText)
        {
            if (!DebugEnabled) return;

            var message = action + " " + additionalText;

            LogMessage(message, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogMessage(string message, string header, DebugColor messageColor, DebugColor headerColor)
        {
            if (!DebugEnabled) return;

            Debug.Log($"<color={DebugColorValue(headerColor)}> [{header}] </color> <color={DebugColorValue(messageColor)}> {message}</color>");
        }

        public static void LogWarning(string message)
        {
            if (!DebugEnabled) return;

            LogWarning(message, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogWarning(string message, string additionalText)
        {
            if (!DebugEnabled) return;

            var result = message + additionalText;

            LogWarning(result, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogWarning(string error, string header, DebugColor messageColor, DebugColor headerColor)
        {
            if (!DebugEnabled) return;

            Debug.LogWarning($"<color={DebugColorValue(headerColor)}> [{header}] </color> <color={DebugColorValue(messageColor)}> {error}</color>");
        }

        public static void LogError(string message)
        {
            LogError(message, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogError(string message, string additionalText)
        {
            var result = message + additionalText ;

            LogError(result, "UISystem", DebugColor.White, DebugColor.Black);
        }

        public static void LogError(string error, string header, DebugColor messageColor, DebugColor headerColor)
        {
            Debug.LogError($"<color={DebugColorValue(headerColor)}> [{header}] </color> <color={DebugColorValue(messageColor)}> {error}</color>");
        }

        private static string DebugColorValue(DebugColor colorEnum)
        {
            return colorEnum switch
            {
                DebugColor.Black => "black",
                DebugColor.Blue => "blue",
                DebugColor.Cyan => "cyan",
                DebugColor.Gray => "gray",
                DebugColor.Green => "green",
                DebugColor.Grey => "grey",
                DebugColor.Magenta => "magenta",
                DebugColor.Red => "red",
                DebugColor.White => "white",
                DebugColor.Yellow => "yellow",

                _ => default
            };
        }
    }
}