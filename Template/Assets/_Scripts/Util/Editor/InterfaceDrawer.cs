using UnityEditor;
using UnityEngine;
using Util.Attributes;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceAttribute), true)]
    public class InterfaceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var interfaceAttr = attribute as InterfaceAttribute;
            var interfaceType = interfaceAttr.InterfaceType;

            var obj = property.objectReferenceValue;
            var objType = obj != null ? obj.GetType() : null;

            if (objType != null && !interfaceType.IsAssignableFrom(objType))
            {
                // If the assigned object doesn't implement the required interface, show an error message
                var errorPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(errorPosition, "Object must implement " + interfaceType.Name, MessageType.Error);
            }

            // Draw the object field for the ScriptableObject
            EditorGUI.PropertyField(position, property, label);

            EditorGUI.EndProperty();
        }
    }
}