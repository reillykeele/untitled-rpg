using UnityEditor;
using UnityEngine;
using Util.Attributes;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(PrefabOnlyAttribute))]
    public class PrefabOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginProperty(position, label, property);

                var asset = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, false);

                if (asset != null && PrefabUtility.GetPrefabAssetType(asset) != PrefabAssetType.Regular)
                {
                    asset = null;
                }

                property.objectReferenceValue = asset;

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [PrefabOnly] with ObjectReference fields.");
            }
        }
    }
}