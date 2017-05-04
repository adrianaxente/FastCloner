using System;
namespace Ax.FastCloner
{
    public class Cloner
    {
        #region Private Static Fields

        private static readonly Lazy<ClonerConfiguration> _defaultConfiguration =
            new Lazy<ClonerConfiguration>(
                () => BuildDefaultConfiguration(),
                true);

        #endregion

        #region Private Fields

        private readonly ClonerConfiguration _configuration;

        #endregion

        #region Constructor

        public Cloner(ClonerConfiguration configuration = null)
        {
            this._configuration =
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

            var context = new ClonerContext();

            var typeCloner = _configuration.GetTypeCloner(instance);
            var clonedInstance = typeCloner(instance, context);

            return (TInstance)clonedInstance;
        }

        #endregion

        #region Private Static Members

        private static ClonerConfiguration BuildDefaultConfiguration()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
