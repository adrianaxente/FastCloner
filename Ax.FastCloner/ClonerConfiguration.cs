using System;
using System.Collections.Generic;

namespace Ax.FastCloner
{
    public abstract class ClonerConfiguration
    {
        #region Private Members

        private readonly IDictionary<Type, TypeCloner> _typeClonerDictionary =
            new Dictionary<Type, TypeCloner>();

        #endregion

        #region Constructor

        public ClonerConfiguration()
        {
        }

        #endregion

        #region Public Methods

        public TypeCloner GetTypeCloner(Type type)
        {
            TypeCloner typeCloner;
            if (!this._typeClonerDictionary.TryGetValue(type, out typeCloner))
            {
                typeCloner = this.BuildTypeCloner(type);
                this._typeClonerDictionary[type] = typeCloner;
            }

            return typeCloner;
        }

        #endregion

        #region Protected Mebers

        protected abstract TypeCloner BuildTypeCloner(Type type);

        #endregion
    }
}
