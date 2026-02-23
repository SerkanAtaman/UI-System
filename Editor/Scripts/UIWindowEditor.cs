using UnityEditor;
using UnityEngine;

namespace SeroJob.UiSystem.Editor
{
    [UnityEditor.CustomEditor(typeof(UIWindow), true)]
    public class UIWindowEditor : UnityEditor.Editor
    {
        protected SerializedProperty windowIDProperty;
        protected SerializedProperty flowDatabaseProperty;
        protected SerializedProperty currentFlowControllerProperty;
        protected SerializedProperty windowStateProperty;
        protected SerializedProperty pagesProperty;
        protected SerializedProperty isScalableProperty;
        protected SerializedProperty scalableElementsProperty;
        protected SerializedProperty PreventBeingHiddenProperty;
        protected SerializedProperty DeactivateOnCloseProperty;
        protected SerializedProperty DisableCanvasOnCloseProperty;

        protected UIWindow targetWindow;

        private bool _settingsFoldout = false;

        public readonly string[] BaseProperties = new string[]
        {
            "m_Script",
            "_windowID",
            "_flowDatabase",
            "windowState",
            "currentFlowController",
            "pages",
            "isScalable",
            "scalableElements",
            "PreventBeingHidden",
            "DeactivateOnClose",
            "DisableCanvasOnClose"
        };

        protected virtual void OnEnable()
        {
            targetWindow = (UIWindow)target;

            windowIDProperty = serializedObject.FindProperty("_windowID");
            flowDatabaseProperty = serializedObject.FindProperty("_flowDatabase");
            windowStateProperty = serializedObject.FindProperty("windowState");
            currentFlowControllerProperty = serializedObject.FindProperty("currentFlowController");
            pagesProperty = serializedObject.FindProperty("pages");
            isScalableProperty = serializedObject.FindProperty("isScalable");
            scalableElementsProperty = serializedObject.FindProperty("scalableElements");
            PreventBeingHiddenProperty = serializedObject.FindProperty("PreventBeingHidden");
            DeactivateOnCloseProperty = serializedObject.FindProperty("DeactivateOnClose");
            DisableCanvasOnCloseProperty = serializedObject.FindProperty("DisableCanvasOnClose");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDisabledGroup();
            DrawMainGroup();
            DrawSettingsGroup();

            DrawPropertiesExcluding(serializedObject, BaseProperties);

            DrawButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDisabledGroup()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), new GUIContent("Script"));
            EditorGUILayout.PropertyField(currentFlowControllerProperty, new GUIContent("Flow Controller"));
            EditorGUILayout.PropertyField(windowStateProperty, new GUIContent("State"));
            EditorGUI.EndDisabledGroup();
        }

        private void DrawMainGroup()
        {
            EditorGUILayout.PropertyField(windowIDProperty, new GUIContent("Window ID"));
            EditorGUILayout.PropertyField(flowDatabaseProperty, new GUIContent("Flow Database"));
            EditorGUILayout.PropertyField(pagesProperty, new GUIContent("Pages"));
        }

        private void DrawSettingsGroup()
        {
            _settingsFoldout = EditorGUILayout.Foldout(_settingsFoldout, new GUIContent("Settings"));
            if (!_settingsFoldout) return;

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(isScalableProperty, new GUIContent("Is Scalable"));
            if (isScalableProperty.boolValue)
            {
                EditorGUILayout.PropertyField(scalableElementsProperty, new GUIContent("Scalable Elements"));
            }

            EditorGUILayout.PropertyField(PreventBeingHiddenProperty, new GUIContent("Prevent Being Hidden"));
            EditorGUILayout.PropertyField(DeactivateOnCloseProperty, new GUIContent("Deactivate On Close"));
            EditorGUILayout.PropertyField(DisableCanvasOnCloseProperty, new GUIContent("Disable Canvas On Close"));

            EditorGUI.indentLevel--;

            if (!isScalableProperty.boolValue)
            {
                scalableElementsProperty.ClearArray();
            }
        }

        private void DrawButtons()
        {
            if (targets.Length > 1) return;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Open"))
            {
                Open();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }

        private void Open()
        {
            if (!Application.isPlaying)
            {
                if (!targetWindow.gameObject.activeSelf) targetWindow.gameObject.SetActive(true);
                targetWindow.GetComponent<Canvas>().enabled = true;

                windowStateProperty.enumValueIndex = 0;

                return;
            }

            targetWindow.OpenImmediately();
        }

        private void Close()
        {
            if (!Application.isPlaying)
            {
                windowStateProperty.enumValueIndex = 2;

                if (DeactivateOnCloseProperty.boolValue) targetWindow.gameObject.SetActive(false);
                if (DisableCanvasOnCloseProperty.boolValue) targetWindow.GetComponent<Canvas>().enabled = false;

                return;
            }

            targetWindow.HideImmediately();
        }
    }
}