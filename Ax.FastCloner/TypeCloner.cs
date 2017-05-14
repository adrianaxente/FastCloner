using System;
namespace Ax.FastCloner
{

    public class TypeCloner
    {
        public delegate object CreateDelegate(object instace, ClonerContext context);
        public delegate void CopyDelegate(object instance, object copyInstance, ClonerContext context);

        #region Properties

        public TypeCloner(
            CreateDelegate creatorDelegate,
            CopyDelegate copierDelegate)
        {
            if (creatorDelegate == null)
            {
                throw new ArgumentNullException(nameof(creatorDelegate));
            }

            this.CreatorDelegate = creatorDelegate;
            this.CopierDelegate = copierDelegate;
        }

        #endregion

        #region Properties

        public CreateDelegate CreatorDelegate
        {
            get;
            private set;
        }

        public CopyDelegate CopierDelegate
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public object Clone(string memberName, object instance, ClonerContext context)
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

                result = CreatorDelegate(instance, context);

                if (CopierDelegate != null)
                {
                    context.VisitedDictionary[instance] = result;
                    CopierDelegate(instance, result, context);
                }

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
