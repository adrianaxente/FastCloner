using System;
namespace Ax.FastCloner
{
    public class Cloner
    {
        #region Private Static Fields

        private static readonly Lazy<ClonerConfiguration> _defaultConfiguration =
            new Lazy<ClonerConfiguration>(
                () => new FieldClonerConfiguration(),
                true);

        #endregion

        #region Constructor

        public Cloner(ClonerConfiguration configuration = null)
        {
            this.Configuration =
                configuration ??
                _defaultConfiguration.Value;
        }

        #endregion

        #region Public Members

        public TInstance Clone<TInstance>(TInstance instance)
        {
            #pragma warning disable RECS0017 // Possible compare of value type with 'null'
            if (instance == null)
            #pragma warning restore RECS0017 // Possible compare of value type with 'null'
            {
                return default(TInstance);
            }

            var context = new ClonerContext(this.Configuration);

            var typeCloner = this.Configuration.GetTypeCloner(instance.GetType());
            var clonedInstance = typeCloner.Clone(null, instance, context);

            return (TInstance)clonedInstance;
        }

        #endregion

        #region Public Properties

        public ClonerConfiguration Configuration
        {
            get;
            private set;
        }

        #endregion
    }
}
