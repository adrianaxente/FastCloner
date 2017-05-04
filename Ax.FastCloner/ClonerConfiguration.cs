using System;
namespace Ax.FastCloner
{
    public class ClonerConfiguration
    {
        #region Constructor

        public ClonerConfiguration()
        {
        }

        #endregion

        #region Public Methods

        public TypeCloner GetTypeCloner(object instance)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
