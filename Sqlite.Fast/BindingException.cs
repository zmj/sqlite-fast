using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// BindingException is thrown when the parameter type cannot be bound to a SQLite statement parameter.
    /// </summary>
    public class BindingException : Exception
    {
        /// <summary>
        /// The name of the parameter type member (null if the parameter type is scalar).
        /// </summary>
        public string? MemberName { get; }

        /// <summary>
        /// The type of the parameter type member.
        /// </summary>
        public Type MemberType { get; }

        /// <summary>
        /// The parameter type.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// The value of the parameter type member.
        /// </summary>
        public object? Value { get; }

        private BindingException(
            string? memberName,
            Type memberType,
            Type parameterType,
            object? value,
            string message)
            : base(message)
        {
            MemberName = memberName;
            MemberType = memberType;
            ParameterType = parameterType;
            Value = value;
        }

        private BindingException(
            string? memberName,
            Type memberType,
            Type parameterType,
            object? value,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            ParameterType = parameterType;
            Value = value;
        }

        internal static void  ThrowConversionMissing<T>(
            string? memberName,
            Type paramsType,
            T value)
        {
            throw new BindingException(
                memberName,
                typeof(T),
                paramsType,
                value,
                $"No defined conversion from value {value} (type {typeof(T).Name}) for {paramsType.Name}.{memberName}");
        }

        internal static void ThrowConversionFailed<T>(
            string? memberName,
            Type paramsType,
            T value,
            Exception innerException)
        {
            throw new BindingException(
                memberName,
                typeof(T),
                paramsType,
                value,
                $"Conversion failed from value {value} (type {typeof(T).Name}) for {paramsType.Name}.{memberName}",
                innerException);
        }
    }
}
