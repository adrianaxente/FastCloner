using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Ax.FastCloner
{
    public class FieldClonerConfiguration : ClonerConfiguration
    {
        #region Private Static Constants

        private static readonly Type[] IMMUTABLE_TYPES =
            new Type[] { typeof(string), typeof(int) };

        #endregion

        #region Constructor

        public FieldClonerConfiguration()
        {
        }

        #endregion

        #region Override

        protected override TypeCloner BuildTypeCloner(Type type)
        {
            var creatorDelegate = GetCreatorDelegate(type);
            var copierDelegate = GetCopierDelegate(type);

            return new TypeCloner(creatorDelegate, copierDelegate);
        }

        #endregion

        #region Private Members

        private TypeCloner.CreateDelegate GetCreatorDelegate(Type type)
        {
            if (IsImutableType(type))
            {
                return
                    new TypeCloner.CreateDelegate(
                        (instance, context) => instance);
            }

            if (type.IsArray)
            {
                //todo: move the above block to a method
				var instanceParameterExpression =
				    Expression.Parameter(typeof(object), "instance");

				var contextParameterExpression =
				    Expression.Parameter(typeof(ClonerContext), "context");

                var bodyStatment =
                    Expression.Condition(
                        Expression.NotEqual(
                            instanceParameterExpression,
                            Expression.Constant(null)),
                        Expression.NewArrayBounds(
                            type.GetElementType(),
                            Expression.ArrayLength(
                                Expression.Convert(
                                    instanceParameterExpression,
                                    type))),
                        Expression.Default(type));

                return
                    Expression
                        .Lambda<TypeCloner.CreateDelegate>(
                            bodyStatment,
                            instanceParameterExpression,
                            contextParameterExpression)
                        .Compile();     
            }

            return
			    new TypeCloner.CreateDelegate(
                    (instance, context) => 
                            instance != null
                                ? FormatterServices.GetUninitializedObject(instance.GetType())
                                : null);
		}

        private TypeCloner.CopyDelegate GetCopierDelegate(Type type)
        {
            if (IsImutableType(type))
            {
                return null;
            }

			var instanceParameterExpression =
				Expression.Parameter(typeof(object), "instance");

			var copyInstanceParameterExpression =
				Expression.Parameter(typeof(object), "copyInstance");

			var contextParameterExpression =
				Expression.Parameter(typeof(ClonerContext), "context");
            
			var typedInstanceVariableExpression =
			   Expression.Variable(type);

			var typedInstanceAssignExpression =
				Expression.Assign(
					typedInstanceVariableExpression,
					Expression.Convert(instanceParameterExpression, type));

			var typedCopyInstanceVariableExpression =
			   Expression.Variable(type);

			var typedCopyInstanceAssignExpression =
				Expression.Assign(
					typedCopyInstanceVariableExpression,
					Expression.Convert(copyInstanceParameterExpression, type));

			var bodyStataments = new List<Expression>
			{
				typedInstanceAssignExpression,
				typedCopyInstanceAssignExpression
			};

            if (type.IsArray)
            {
                var arrayCopyStataments =
                    this.GetArrayCloneExpressions(
                        typedInstanceVariableExpression,
                        typedCopyInstanceVariableExpression,
                        contextParameterExpression);
                
                bodyStataments.AddRange(arrayCopyStataments);
            }
            else
            {

                var fieldsToClone =
                    type.GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.FlattenHierarchy);




                var fieldCloneStatments =
                    fieldsToClone
                        .SelectMany(fi => GetFieldCloneExpressions(
                                            fi,
                                            typedInstanceVariableExpression,
                                            typedCopyInstanceVariableExpression,
                                            contextParameterExpression));

                bodyStataments.AddRange(fieldCloneStatments);
            }

            var bodyExpression =
                Expression.Block(
                    new[] {
                            typedInstanceVariableExpression,
                            typedCopyInstanceVariableExpression
                          },
                    bodyStataments);

            return
                Expression
                    .Lambda<TypeCloner.CopyDelegate>(
                        bodyExpression,
                        instanceParameterExpression,
                        copyInstanceParameterExpression,
                        contextParameterExpression)
                    .Compile();
        }

        private IEnumerable<Expression> GetFieldCloneExpressions(
            FieldInfo fieldInfo,
            ParameterExpression typedInstanceVariableExpression,
            ParameterExpression typedCopyInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
			yield return
				 Expression.Assign(
                    Expression.Field(
					    typedCopyInstanceVariableExpression,
						fieldInfo),
                    Expression.Convert(
                        GetCloneCallExpression(
                            Expression.Field(
                                typedInstanceVariableExpression,
                                fieldInfo),
                            contextParameterExpresion,
                            fieldInfo.Name),
                        fieldInfo.FieldType));
        }

        private IEnumerable<Expression> GetArrayCloneExpressions(
            ParameterExpression typedInstanceVariableExpression,
            ParameterExpression typedCopyInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
            

            var loopVariableExpression = Expression.Variable(typeof(int));
            var breakLabelExpression = Expression.Label();

            var loopVariableAssigmentExpression =
                Expression
                    .Assign(
                        loopVariableExpression,
                        Expression.Constant(0, typeof(int)));

            var copyLoopExpression =
	            Expression.Loop(
	                Expression
	                    .IfThenElse(
	                        Expression.LessThan(
	                            loopVariableExpression, 
	                            Expression.ArrayLength(typedInstanceVariableExpression)),
                            Expression.Block(
		                        Expression.Assign(
		                            Expression.ArrayAccess(
		                                typedCopyInstanceVariableExpression,
		                                loopVariableExpression),
                                    Expression.Convert(
                                        GetCloneCallExpression(
								            Expression.ArrayAccess(
									            typedInstanceVariableExpression,
                                                loopVariableExpression),
                                            contextParameterExpresion,
                                        string.Empty),
                                        typedInstanceVariableExpression.Type.GetElementType())),
                                Expression.PostIncrementAssign(loopVariableExpression)),
	                        Expression.Break(breakLabelExpression)),
	                breakLabelExpression);

            yield return Expression.Block(
                new[] { loopVariableExpression },
                loopVariableAssigmentExpression,
                copyLoopExpression);

        }

        private bool IsImutableType(Type type)
        {
            return IMMUTABLE_TYPES.Contains(type);
        }

		private Expression GetCloneCallExpression(
			Expression typedInstanceVariableExpression,
			ParameterExpression contextParameterExpresion,
			string memberName)
		{
			return
				Expression.Call(
					Expression.Call(
						Expression.Property(
							contextParameterExpresion,
							nameof(ClonerContext.Configuration)),
						typeof(ClonerConfiguration).GetMethod(nameof(ClonerConfiguration.GetTypeCloner)),
						Expression.Call(
							typedInstanceVariableExpression,
							typeof(object).GetMethod(nameof(object.GetType)))),
					typeof(TypeCloner).GetMethod(nameof(TypeCloner.Clone)),
					Expression.Constant(memberName),
					Expression.Convert(
						typedInstanceVariableExpression,
						typeof(object)),
					contextParameterExpresion);
		}

        #endregion
    }
}
