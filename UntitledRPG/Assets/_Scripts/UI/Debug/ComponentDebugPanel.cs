using System;
using System.Linq;
using System.Reflection;
using ReiBrary.Attributes;
using TMPro;
using UnityEngine;

namespace UntitledRPG.UI.Debug
{
    public class ComponentDebugPanel : MonoBehaviour
    {
        [SerializeField] private string _componentType;
        [SerializeField] private GameObject _gameObject;

        private Component _component;

        private TextMeshProUGUI[] _texts;

        private PropertyInfo[] _properties;

        void Awake()
        {
            UnityEngine.Debug.Log(Type.GetType(_componentType)?.ToString());

            _gameObject.TryGetComponent(Type.GetType(_componentType), out _component);

            _properties = _component.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(DebuggableAttribute))).ToArray();

            _texts = new TextMeshProUGUI[_properties.Length];
            _texts[0] = GetComponentInChildren<TextMeshProUGUI>();
            for (int i = 1; i < _properties.Length; i++)
            {
                _texts[i] = Instantiate(_texts[0], transform);
            }
        }

        void Update()
        {
            for (var i = 0; i <  _properties.Length; i++)
            {
                _texts[i].text = GetPropertyString(_properties[i]);
            }
        }

        private string GetPropertyString(PropertyInfo prop)
        {
            var propName = prop.Name;
            var propValue = prop.GetValue(_component);

            if (prop.PropertyType == typeof(bool))
            {
                return (bool) propValue ?
                    $"{propName}: <color=\"green\">{propValue}</color>" :
                    $"{propName}: <color=\"red\">{propValue}</color>";
            }

            if (prop.PropertyType == typeof(float))
            {
                return $"{propName}: {propValue:F3}";
            }

            return $"{propName}: {propValue}";
        }
    }
}