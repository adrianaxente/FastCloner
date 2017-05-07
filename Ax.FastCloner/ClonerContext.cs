using System;
using System.Collections.Generic;

namespace Ax.FastCloner
{
    public class ClonerContext
    {
        #region Constructor

        public ClonerContext(ClonerConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.Configuration = configuration;
        }

        #endregion

        #region Public Properties

        public ClonerConfiguration Configuration
        {
            get;
            protected set;
        }

        public IDictionary<object, object> VisitedDictionary
        {
            get;
            private set;
        } = new Dictionary<object, object>();

        public Stack<string> PathStack
        {
            get;
            private set;
        } = new Stack<string>();

        #endregion
    }
}
