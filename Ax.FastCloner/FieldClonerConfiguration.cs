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
            return
                IsImutableType(type)
                    ? new TypeCloner.CreateDelegate(
                        (instance, context) => instance) 
                    : new TypeCloner.CreateDelegate(
                        (instance, context) => FormatterServices.GetUninitializedObject(instance.GetType())); 
        }

        private TypeCloner.CopyDelegate GetCopierDelegate(Type type)
        {
            if (IsImutableType(type))
            {
                return null;
            }

            var fieldsToClone =
                type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.FlattenHierarchy);

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


            var fieldCloneStatments =
                fieldsToClone
                    .SelectMany(fi => GetFieldCloneExpressions(
                                        fi,
                                        typedInstanceVariableExpression,
                                        typedCopyInstanceVariableExpression,
                                        contextParameterExpression));

            bodyStataments.AddRange(fieldCloneStatments);

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
            if (fieldInfo.FieldType.IsArray)
            {
                return
                    GetArrayCloneExpressions(
                        fieldInfo,
                        typedInstanceVariableExpression,
                        typedCopyInstanceVariableExpression,
                        contextParameterExpresion);
            }

            return
                GetCloneExpressions(
                    fieldInfo,
                    typedInstanceVariableExpression,
                    typedCopyInstanceVariableExpression,
                    contextParameterExpresion);
        }

        private IEnumerable<Expression> GetCloneExpressions(
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
                                Expression.Convert(
                                    Expression.Field(
                                        typedInstanceVariableExpression,
                                        fieldInfo),
                                    typeof(object)),
                                contextParameterExpresion),
                        fieldInfo.FieldType));
        }

        private IEnumerable<Expression> GetArrayCloneExpressions(
            FieldInfo fieldInfo,
            ParameterExpression typedInstanceVariableExpression,
            ParameterExpression typedCopyInstanceVariableExpression,
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

        private bool IsImutableType(Type type)
        {
            return IMMUTABLE_TYPES.Contains(type);
        }

        #endregion
    }
}
