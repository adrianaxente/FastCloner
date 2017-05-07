using System;
namespace Ax.FastCloner
{
    public delegate object TypeClonerDelegate(object instace, ClonerContext context);

    public class TypeCloner
    {
        #region Properties

        public TypeCloner(TypeClonerDelegate clonerDelegate)
        {
            if (clonerDelegate == null)
            {
                throw new ArgumentNullException(nameof(clonerDelegate));
            }

            this.ClonerDelegate = clonerDelegate;
        }

        #endregion

        #region Properties

        public TypeClonerDelegate ClonerDelegate
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public virtual object Clone(string memberName, object instance, ClonerContext context)
        {
            if (instance == null)
            {
                return null;
            }

            object result;

            if (!context.VisitedDictionary.TryGetValue(instance, out result))
            {
                if (!string.IsNullOrEmpty(memberName))
                {
                    context.PathStack.Push(memberName);
                }

                result = ClonerDelegate(instance, context);

                context.VisitedDictionary[instance] = result;

                if (!string.IsNullOrEmpty(memberName))
                {
                    context.PathStack.Pop();
                }
            }

            return result;
        }

        #endregion
    }

}
