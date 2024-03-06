using UnityEditor;
using UnityEngine;

namespace SeroJob.UiSystem.Editor
{
    [CustomPropertyDrawer(typeof(UIWindowReference))]
    public class WindowReferenceDrawer : PropertyDrawer
    {
        private int _totalLinesToDraw;
        private int _index;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Selection.count > 1) return;

            SetSelectedWindowIndex(property);

            int verticalIndex = 0;
            int preIntent = EditorGUI.indentLevel;

            var rectFoldout = new Rect(position.min.x, position.min.y, position.size.x,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);

            if (property.isExpanded)
            {
                verticalIndex++;
                EditorGUI.indentLevel++;

                DrawWindowReferenceProperty(position, property, label, verticalIndex);
            }

            EditorGUI.EndProperty();
            EditorGUI.indentLevel = preIntent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SetTotalLinesToDraw(property);

            return EditorGUIUtility.singleLineHeight * _totalLinesToDraw +
                   EditorGUIUtility.standardVerticalSpacing * (_totalLinesToDraw - 1);
        }

        private void SetTotalLinesToDraw(SerializedProperty property)
        {
            _totalLinesToDraw = 1;
            if (!property.isExpanded) return;

            _totalLinesToDraw = 2;
        }

        private void SetSelectedWindowIndex(SerializedProperty property)
        {
            var windowProperty = property.FindPropertyRelative("_windowID");

            if (string.IsNullOrEmpty(windowProperty.stringValue))
            {
                _index = 0;
                return;
            }

            _index = GetIndexOfWindow(property, windowProperty.stringValue);
        }

        private void DrawWindowReferenceProperty(Rect position, SerializedProperty property, GUIContent label, int line)
        {
            var windowProperty = property.FindPropertyRelative("_windowID");
            var rectDatabaseProperty = new Rect(position.min.x,
                position.min.y + line * EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing * line,
                position.size.x,
                EditorGUIUtility.singleLineHeight);

            var windows = GetWindowReferences();

            if(windows == null)
            {
                windowProperty.stringValue = null;
                return;
            }

            _index = EditorGUI.Popup(rectDatabaseProperty, "Window", _index, windows);

            if (_index < 0) _index = 0;
            if(_index >= windows.Length) _index = windows.Length - 1;

            windowProperty.stringValue = windows[_index];
        }

        private string[] GetWindowReferences()
        {
            if (Selection.count > 1) return null;

            FlowDatabase database = null;

            if(Selection.activeGameObject.TryGetComponent(out IFlowProvider provider))
            {
                database = provider.GetFlowDatabase();
            }

            if (database == null) return null;

            return database.WindowIDs;
        }

        private int GetIndexOfWindow(SerializedProperty property, string targetWindow)
        {
            string[] windows = GetWindowReferences();

            if (windows == null) return 0;
            if (windows.Length == 0) return 0;

            int index = 0;

            for(int i = 0; i < windows.Length; i++)
            {
                if (string.Equals(windows[i], targetWindow))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
    }
}