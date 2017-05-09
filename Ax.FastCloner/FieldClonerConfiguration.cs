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
            var fieldsToClone =
                type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);

            var instanceParameterExpression =
                Expression.Parameter(typeof(object), "instance");

            var contextParameterExpression = 
                Expression.Parameter(typeof(ClonerContext), "context");

            var typedInstanceVariableExpression =
                Expression.Variable(type);

            var typedInstanceAssignExpression =
                Expression.Assign(
                    typedInstanceVariableExpression,
                    Expression.Convert(instanceParameterExpression, type));

            var clonedInstanceVariableExpression =
                Expression.Variable(type);

            var clonedInstanceCreationExpression =
                Expression.Assign(
                    clonedInstanceVariableExpression,
                    Expression.Convert(
	                    Expression.Call(
	                        typeof(FormatterServices)
	                           .GetMethod(nameof(FormatterServices.GetUninitializedObject)),
	                        Expression.Call(
	                            instanceParameterExpression,
                                typeof(object).GetMethod(nameof(object.GetType)))),
                        type));

            var bodyStataments = new List<Expression>
            {
                typedInstanceAssignExpression,
                clonedInstanceCreationExpression
            };


            var fieldCloneStatments =
                fieldsToClone
                    .SelectMany(fi => GetFieldCloneExpressions(
                                        fi, 
                                        clonedInstanceVariableExpression,
                                        typedInstanceVariableExpression, 
                                        contextParameterExpression));                            

            bodyStataments.AddRange(fieldCloneStatments);

            bodyStataments.Add(clonedInstanceVariableExpression);

            var bodyExpression = 
                Expression.Block(
                    new[] {
                            typedInstanceVariableExpression,
                            clonedInstanceVariableExpression 
                          },
                    bodyStataments);

            var clonerDelegate =
                Expression
                    .Lambda<TypeClonerDelegate>(
                        bodyExpression,
                        instanceParameterExpression,
                        contextParameterExpression)
                    .Compile();

            return new TypeCloner(clonerDelegate);
        }

        #endregion

        #region Private Members

        private IEnumerable<Expression> GetFieldCloneExpressions(
            FieldInfo fieldInfo,
            ParameterExpression clonedInstanceVariableExpression,
            ParameterExpression typedInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
            if (IMMUTABLE_TYPES.Contains(fieldInfo.FieldType))
            {
                return GetAssigmentExpressions(
                    fieldInfo,
                    clonedInstanceVariableExpression,
                    typedInstanceVariableExpression);
            }

            if (fieldInfo.FieldType.IsArray)
            {
                return
                    GetArrayCloneExpressions(
					    fieldInfo,
                        clonedInstanceVariableExpression,
                        typedInstanceVariableExpression,
                        contextParameterExpresion);
            }

                return
                    GetCloneExpressions(
                        fieldInfo,
                        clonedInstanceVariableExpression,
                        typedInstanceVariableExpression,
                        contextParameterExpresion);
        }

        private IEnumerable<Expression> GetAssigmentExpressions(
		    FieldInfo fieldInfo,
            ParameterExpression clonedInstanceVariableExpression,
			ParameterExpression typedInstanceVariableExpression)
        {
            var leftExpression =
                Expression
                    .Field(clonedInstanceVariableExpression, fieldInfo);

            var rightExpression =
                Expression
                    .Field(typedInstanceVariableExpression, fieldInfo);

            yield return Expression.Assign(leftExpression, rightExpression);
        }

		private IEnumerable<Expression> GetCloneExpressions(
			FieldInfo fieldInfo,
			ParameterExpression clonedInstanceVariableExpression,
			ParameterExpression typedInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
            yield return
                Expression.IfThen(
                    Expression.NotEqual(
                        Expression.Field(
                            typedInstanceVariableExpression,
                            fieldInfo),
                        Expression.Constant(null)),
                    Expression.Assign(
                        Expression.Field(
                            clonedInstanceVariableExpression,
                            fieldInfo),
                        Expression.Convert(
                            Expression.Call(
                                Expression.Call(
                                    Expression.Property(
                                        contextParameterExpresion,
                                        nameof(ClonerContext.Configuration)),
                                    typeof(ClonerConfiguration).GetMethod(nameof(ClonerConfiguration.GetTypeCloner)),
                                    Expression.Call(
                                        Expression.Field(
                                            typedInstanceVariableExpression,
                                            fieldInfo),
                                        typeof(object).GetMethod(nameof(object.GetType)))),
                                typeof(TypeCloner).GetMethod(nameof(TypeCloner.Clone)),
                                Expression.Constant(fieldInfo.Name),
                                Expression.Field(
                                    typedInstanceVariableExpression,
                                    fieldInfo),
                                contextParameterExpresion),
                            fieldInfo.FieldType)));
        }

		private IEnumerable<Expression> GetArrayCloneExpressions(
			FieldInfo fieldInfo,
			ParameterExpression clonedInstanceVariableExpression,
			ParameterExpression typedInstanceVariableExpression,
			ParameterExpression contextParameterExpresion)
        {
            /*var instanceFieldExpression = 
                Expression
                    .Field(
                        typedInstanceVariableExpression,
                        fieldInfo);

            var clonedInstanceFieldExpression =
                Expression
                    .Field(clonedInstanceVariableExpression, fieldInfo);

            var clonedFieldAssignmentExpression =
                Expression
                    .Assign(
                        clonedInstanceFieldExpression,
                        Expression.NewArrayBounds(
                            fieldInfo.FieldType.GetElementType(),
                            Expression.ArrayLength(instanceFieldExpression)));

            var loopVariableExpression = Expression.Variable(typeof(int));
            var breakLabelExpression = Expression.Label(typeof(int));

            var loopVariableAssigmentExpression =
                Expression
                    .Assign(
                        loopVariableExpression,
                        Expression.Constant(0, typeof(int)));

            Expression.Loop(
                Expression
                    .IfThenElse(
                        Expression.LessThan(
                            loopVariableExpression, 
                            Expression.ArrayLength(instanceFieldExpression)),
                        ),
                breakLabelExpression);

            yield return clonedFieldAssignmentExpression;*/

            //TODO: Finalize this method
            yield break;
        }

        #endregion
    }
}
