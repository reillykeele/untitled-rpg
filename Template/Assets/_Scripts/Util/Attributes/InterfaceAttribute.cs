using UnityEngine;

namespace Util.Attributes
{
    public class InterfaceAttribute : PropertyAttribute
    {
        public readonly System.Type InterfaceType;

        public InterfaceAttribute(System.Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new System.ArgumentException("Type is not an interface");

            InterfaceType = interfaceType;
        }
    }
}