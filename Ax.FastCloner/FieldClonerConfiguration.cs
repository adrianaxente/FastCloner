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
	                            typeof(object).GetMethod("GetType"))),
                        type));

            var bodyStataments = new List<Expression>
            {
                typedInstanceAssignExpression,
                clonedInstanceCreationExpression
            };


            var fieldCloneStatments =
                fieldsToClone
                    .Select(fi => GetFieldCloneExpression(
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

        private Expression GetFieldCloneExpression(
            FieldInfo fieldInfo,
            ParameterExpression clonedInstanceVariableExpression,
            ParameterExpression typedInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
            if (IMMUTABLE_TYPES.Contains(fieldInfo.FieldType))
            {
                return GetAssigmentExpression(
                    fieldInfo,
                    clonedInstanceVariableExpression,
                    typedInstanceVariableExpression);
            }

            return
                GetCloneExpression(
                    fieldInfo,
                    clonedInstanceVariableExpression,
                    typedInstanceVariableExpression,
                    contextParameterExpresion);
        }

        private Expression GetAssigmentExpression(
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

            return
                Expression.Assign(leftExpression, rightExpression);
        }

		private Expression GetCloneExpression(
			FieldInfo fieldInfo,
			ParameterExpression clonedInstanceVariableExpression,
			ParameterExpression typedInstanceVariableExpression,
            ParameterExpression contextParameterExpresion)
        {
            return
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
                                        "Configuration"),
                                    typeof(ClonerConfiguration).GetMethod("GetTypeCloner"),
                                    Expression.Call(
                                        Expression.Field(
                                            typedInstanceVariableExpression,
                                            fieldInfo),
                                        typeof(object).GetMethod("GetType"))),
                                typeof(TypeCloner).GetMethod("Clone"),
                                Expression.Constant(fieldInfo.Name),
                                Expression.Field(
                                    typedInstanceVariableExpression,
                                    fieldInfo),
                                contextParameterExpresion),
                            fieldInfo.FieldType)));
        }

        #endregion
    }
}
