using System;
using System.Reflection;

namespace Ax.FastCloner
{
    public class FieldClonerConfiguration : ClonerConfiguration
    {
        public FieldClonerConfiguration()
        {
        }

        protected override TypeCloner BuildTypeCloner(Type type)
        {
            var fieldsToClone =
                type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);


            foreach (var field in fieldsToClone)
            {
                
            }

            return null;
        }
    }
}
