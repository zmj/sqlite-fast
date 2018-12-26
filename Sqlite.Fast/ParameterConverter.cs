using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class ParameterConverter<TParams>
    {
        private readonly IValueBinder<TParams>[] _binders;
        private readonly ValueBinder<TParams, TParams> _scalarBinder;
        internal readonly int FieldCount;

        internal ParameterConverter(IEnumerable<IValueBinder<TParams>> valueBinders)
        {
            _binders = valueBinders.ToArray();
            FieldCount = _binders.Length;
        }

        internal ParameterConverter(ValueBinder<TParams, TParams> scalarBinder)
        {
            _scalarBinder = scalarBinder;
            FieldCount = 1;
        }

        internal void BindValues(in TParams parameters, Statement statement)
        {
            if (_scalarBinder != null)
            {
                _scalarBinder.Bind(in parameters, statement, index: 1);
            }
            else
            {
                for (int i = 0; i < _binders.Length; i++)
                {
                    // parameters are 1-indexed
                    _binders[i].Bind(in parameters, statement, i + 1);
                }
            }
        }

        public sealed class Builder
        {
            private readonly List<ValueBinder.IBuilder<TParams>> _binderBuilders;
            private readonly bool _withDefaults;

            public Builder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _binderBuilders = typeof(TParams)
                    .FieldsOrderedByDeclaration()
                    .Where(CanGetValue)
                    .Select(m => ValueBinder.Build<TParams>(m))
                    .ToList();
            }

            private ValueBinder.IBuilder<TParams> GetOrAdd(MemberInfo member)
            {
                var builder = _binderBuilders.Find(cm => cm.Member == member);
                if (builder == null)
                {
                    builder = ValueBinder.Build<TParams>(member);
                    _binderBuilders.Add(builder);
                }
                return builder;
            }

            private ValueBinder.Builder<TParams, TField> GetOrAdd<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                if (GetGettableMember(propertyOrField, out MemberInfo member))
                {
                    return GetOrAdd(member).AsConcrete<TField>();
                }
                throw new ArgumentException($"Expression is not gettable field or property of {typeof(TParams).Name}");
            }

            public ParameterConverter<TParams> Compile()
            {
                return new ParameterConverter<TParams>(_binderBuilders.Select(b => b.Compile(_withDefaults)));
            }
            
            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToInteger<TField> toInteger)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToInteger<TField> toInteger)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToFloat<TField> toFloat)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField, 
                Func<TField, bool> canConvert, 
                ToFloat<TField> toFloat)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToText<TField> toText,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(toText, length));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToText<TField> toText,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, toText, length));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                AsText<TField> asText)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(asText));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                AsText<TField> asText)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, asText));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToBlob<TField> toBlob,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(toBlob, length));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToBlob<TField> toBlob,
                Func<TField, int> length)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(canConvert, toBlob, length));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                AsBlob<TField> asBlob)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(asBlob));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                AsBlob<TField> asBlob)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Blob(canConvert, asBlob));
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                ToNull<TField> toNull)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null<TField>());
                return this;
            }

            public Builder With<TField>(
                Expression<Func<TParams, TField>> propertyOrField,
                Func<TField, bool> canConvert,
                ToNull<TField> toNull)
            {
                GetOrAdd(propertyOrField).Converters.Add(ValueBinder.Converter.Null(canConvert));
                return this;
            }

            public Builder Ignore<TField>(Expression<Func<TParams, TField>> propertyOrField)
            {
                if (GetGettableMember(propertyOrField, out MemberInfo member))
                {
                    _binderBuilders.RemoveAll(builder => builder.Member == member);
                }
                return this;
            }

            private static bool GetGettableMember<TField>(Expression<Func<TParams, TField>> propertyOrField, out MemberInfo member)
            {
                if (propertyOrField.Body is MemberExpression memberExpression && CanGetValue(memberExpression.Member))
                {
                    member = memberExpression.Member;
                    return true;
                }
                member = null;
                return false;
            }

            private static bool CanGetValue(MemberInfo member)
            {
                if (member is PropertyInfo property)
                {
                    return property.CanRead;
                }
                else if (member is FieldInfo field)
                {
                    return true;
                }
                return false;
            }
        }

        public sealed class ScalarBuilder
        {
            private readonly ValueBinder.Builder<TParams, TParams> _builder;
            private readonly bool _withDefaults;

            public ScalarBuilder(bool withDefaultConversions = true)
            {
                _withDefaults = withDefaultConversions;
                _builder = ValueBinder.Build<TParams>();
            }

            public ParameterConverter<TParams> Compile() =>
                new ParameterConverter<TParams>(_builder.Compile(_withDefaults));

            public ScalarBuilder With(ToInteger<TParams> toInteger)
            {
                _builder.Converters.Add(ValueBinder.Converter.Integer(toInteger));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToInteger<TParams> toInteger)
            {
                _builder.Converters.Add(ValueBinder.Converter.Integer(canConvert, toInteger));
                return this;
            }

            public ScalarBuilder With(ToFloat<TParams> toFloat)
            {
                _builder.Converters.Add(ValueBinder.Converter.Float(toFloat));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToFloat<TParams> toFloat)
            {
                _builder.Converters.Add(ValueBinder.Converter.Float(canConvert, toFloat));
                return this;
            }

            public ScalarBuilder With(ToText<TParams> toText, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(toText, length));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToText<TParams> toText, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, toText, length));
                return this;
            }

            public ScalarBuilder With(AsText<TParams> asText)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(asText));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, AsText<TParams> asText)
            {
                _builder.Converters.Add(ValueBinder.Converter.Utf16Text(canConvert, asText));
                return this;
            }

            public ScalarBuilder With(ToBlob<TParams> toBlob, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(toBlob, length));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, ToBlob<TParams> toBlob, Func<TParams, int> length)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, toBlob, length));
                return this;
            }

            public ScalarBuilder With(AsBlob<TParams> asBlob)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(asBlob));
                return this;
            }

            public ScalarBuilder With(Func<TParams, bool> canConvert, AsBlob<TParams> asBlob)
            {
                _builder.Converters.Add(ValueBinder.Converter.Blob(canConvert, asBlob));
                return this;
            }
        }
    }

    public static class ParameterConverter
    {
        public static ParameterConverter<TParams>.Builder Builder<TParams>(bool withDefaultConversions = true) =>
            new ParameterConverter<TParams>.Builder(withDefaultConversions);

        public static ParameterConverter<TParams>.ScalarBuilder ScalarBuilder<TParams>(bool withDefaultConversions = true) =>
            new ParameterConverter<TParams>.ScalarBuilder(withDefaultConversions);

        internal static ParameterConverter<TParams> Default<TParams>()
        {
            if (typeof(TParams).IsScalar())
            {
                return ScalarBuilder<TParams>().Compile();
            }
            else
            {
                return Builder<TParams>().Compile();
            }
        }
    }
}
