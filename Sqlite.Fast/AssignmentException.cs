using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// AssignmentException is thrown when a SQLite value cannot be assigned to the result type.
    /// </summary>
    public class AssignmentException : Exception
    {
        /// <summary>
        /// The name of the result type member (null if the result type is scalar).
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// The type of the result type member.
        /// </summary>
        public Type MemberType { get; }

        /// <summary>
        /// The result type.
        /// </summary>
        public Type ResultType { get; }

        /// <summary>
        /// The type of the SQLite value.
        /// </summary>
        public string DataType { get; }

        private AssignmentException(
            string memberName, 
            Type memberType, 
            Type resultType, 
            Sqlite.DataType dataType,
            string message)
            : base(message)
        {
            MemberName = memberName;
            MemberType = memberType;
            ResultType = resultType;
            DataType = dataType.ToString();
        }

        private AssignmentException(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            ResultType = resultType;
            DataType = dataType.ToString();
        }

        internal static AssignmentException ConversionMissing(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType)
        {
            return new AssignmentException(
                memberName,
                memberType,
                resultType,
                dataType,
                $"No defined conversion from SQLite.{dataType} to {memberType.Name} for {resultType.Name}.{memberName}. (Add custom conversions with {nameof(ResultConverter)}.{nameof(ResultConverter.Builder)}.)");
        }

        internal static AssignmentException ConversionFailed(
            string memberName,
            Type memberType,
            Type resultType,
            Sqlite.DataType dataType,
            Exception innerException)
        {
            return new AssignmentException(
                memberName,
                memberType,
                resultType,
                dataType,
                $"Conversion failed from SQLite.{dataType} to {memberType.Name} for {resultType.Name}.{memberName}.",
                innerException);
        }
    }
}
