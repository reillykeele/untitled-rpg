using UnityEditor;
using UnityEngine;
using Util.Attributes;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
    public class UniqueIdentifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var assetPath = AssetDatabase.GetAssetPath(prop.serializedObject.targetObject.GetInstanceID());
            var uniqueId = AssetDatabase.AssetPathToGUID(assetPath);

            prop.stringValue = uniqueId;

            var textFieldPosition = position;
            textFieldPosition.height = 16;
            DrawLabelField(textFieldPosition, prop, label);
        }

        void DrawLabelField(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.LabelField(position, label, new GUIContent(prop.stringValue));
        }

    }
}
