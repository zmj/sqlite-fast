using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Sqlite.Fast
{
    public sealed partial class ResultConverter<TResult>
    {
        public sealed class Builder
        {
            private readonly List<ValueAssigner.IBuilder<TResult>> _assignerBuilders = new List<ValueAssigner.IBuilder<TResult>>();
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _assignerBuilders = typeof(TResult)
                    .FieldsOrderedByDeclaration()
                    .Where(CanSetValue)
                    .Select(m => ValueAssigner.Build<TResult>(m))
                    .ToList();
            }

            private ValueAssigner.IBuilder<TResult> GetOrAdd(MemberInfo member)
            {
                var builder = _assignerBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueAssigner.Build<TResult>(member);
                    _assignerBuilders.Add(builder);
                }
                return builder;
            }

            private ValueAssigner.Builder<TResult, TField> GetOrAdd<TField>(Expression<Func<TResult, TField>> propertyOrField)
            {
                if (GetSettableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not settable field or property of {typeof(TResult).Name}");
            }

            public ResultConverter<TResult> Compile()
            {
                return new ResultConverter<TResult>(_assignerBuilders.Select(b => b.Compile(_withDefaults)));
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromInteger<TField> fromInteger)
            {
                GetOrAdd(propertyOrField).FromInteger = fromInteger;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromFloat<TField> fromFloat)
            {
                GetOrAdd(propertyOrField).FromFloat = fromFloat;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromText<TField> fromText)
            {
                GetOrAdd(propertyOrField).FromUtf16Text = fromText;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromBlob<TField> fromBlob)
            {
                GetOrAdd(propertyOrField).FromBlob = fromBlob;
                return this;
            }

            public Builder With<TField>(Expression<Func<TResult, TField>> propertyOrField, FromNull<TField> fromNull)
            {
                GetOrAdd(propertyOrField).FromNull = fromNull;
                return this;
            }

            public Builder Ignore<TField>(Expression<Func<TResult, TField>> propertyOrField)
            {
                if (GetSettableMember(propertyOrField, out MemberInfo member))
                {
                    _assignerBuilders.RemoveAll(builder => builder.Member == member);
                }
                return this;
            }

            private static bool GetSettableMember<TField>(Expression<Func<TResult, TField>> propertyOrField, out MemberInfo member)
            {
                if (propertyOrField.Body is MemberExpression memberExpression && CanSetValue(memberExpression.Member))
                {
                    member = memberExpression.Member;
                    return true;
                }
                member = null;
                return false;
            }

            private static bool CanSetValue(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanWrite;
                }
                else if (member is FieldInfo field)
                {
                    return !field.IsInitOnly;
                }
                return false;
            }
        }
    }
}
