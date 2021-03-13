using Caxapexac.Common.Sharp.Runtime.Extensions;
using MyBox.Internal;
using UnityEditor;
using UnityEngine;


namespace Caxapexac.Common.Sharp.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(DisplayInspectorInsideAttribute))]
    public class DisplayInspectorInsideInspector : PropertyDrawer
    {
        private DisplayInspectorInsideAttribute _instance;
        private ButtonInspector _buttonMethods;

        private DisplayInspectorInsideAttribute Instance
        {
            get => _instance ?? (_instance = attribute as DisplayInspectorInsideAttribute);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Instance.DisplayScript || property.objectReferenceValue == null)
            {
                position.height = EditorGUI.GetPropertyHeight(property);
                EditorGUI.PropertyField(position, property, label);
                position.y += EditorGUI.GetPropertyHeight(property) + 4;
            }

            if (property.objectReferenceValue != null)
            {
                if (_buttonMethods == null) _buttonMethods = new ButtonInspector(property.objectReferenceValue);

                var startY = position.y;
                float startX = position.x;

                var propertyObject = new SerializedObject(property.objectReferenceValue).GetIterator();
                propertyObject.Next(true);
                propertyObject.NextVisible(false);

                var xPos = position.x + 10;
                var width = position.width - 10;

                while (propertyObject.NextVisible(propertyObject.isExpanded))
                {
                    position.x = xPos + 10 * propertyObject.depth;
                    position.width = width - 10 * propertyObject.depth;

                    position.height = propertyObject.isExpanded ? 16 : EditorGUI.GetPropertyHeight(propertyObject);
                    EditorGUI.PropertyField(position, propertyObject);
                    position.y += propertyObject.isExpanded ? 20 : EditorGUI.GetPropertyHeight(propertyObject) + 4;
                }

                if (!(_buttonMethods.TargetMethods == null || _buttonMethods.TargetMethods.Count == 0))
                {
                    foreach (var method in _buttonMethods.TargetMethods)
                    {
                        position.height = EditorGUIUtility.singleLineHeight;
                        if (GUI.Button(position, method.Name)) _buttonMethods.Invoke(method.Method);
                        position.y += position.height;
                    }
                }

                var bgRect = position;
                bgRect.y = startY - 5;
                bgRect.x = startX - 10;
                bgRect.width = 10;
                bgRect.height = position.y - startY;
                if (_buttonMethods.Amount > 0) bgRect.height += 5;
                DrawColouredRect(bgRect, new Color(.6f, .6f, .8f, .5f));
                if (GUI.changed) propertyObject.serializedObject.ApplyModifiedProperties();
            }

            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null) return base.GetPropertyHeight(property, label);
            if (_buttonMethods == null) _buttonMethods = new ButtonInspector(property.objectReferenceValue);

            float height = Instance.DisplayScript ? EditorGUI.GetPropertyHeight(property) + 4 : 0;

            var propertyObject = new SerializedObject(property.objectReferenceValue).GetIterator();
            propertyObject.Next(true);
            propertyObject.NextVisible(true);

            while (propertyObject.NextVisible(propertyObject.isExpanded))
            {
                height += propertyObject.isExpanded ? 20 : EditorGUI.GetPropertyHeight(propertyObject) + 4;
            }

            if (_buttonMethods.Amount > 0) height += 4 + _buttonMethods.Amount * EditorGUIUtility.singleLineHeight;
            return height;
        }

        private void DrawColouredRect(Rect rect, Color color)
        {
            var defaultBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, "");
            GUI.backgroundColor = defaultBackgroundColor;
        }
    }
}