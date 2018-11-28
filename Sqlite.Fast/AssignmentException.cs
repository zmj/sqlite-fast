using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class AssignmentException : Exception
    {
        public string MemberName { get; }
        public Type MemberType { get; }
        public Type RecordType { get; }
        public string DataType { get; }

        internal AssignmentException(
            string memberName, 
            Type memberType, 
            Type recordType, 
            DataType dataType)
            : base($"No defined conversion from Sqlite.{dataType} to {memberType.Name} for {recordType.Name}.{memberName}. (Add custom conversions when building the {nameof(RecordConverter)}.)")
        {
            MemberName = memberName;
            MemberType = memberType;
            RecordType = recordType;
            DataType = dataType.ToString();
        }

        internal AssignmentException(
            string memberName,
            Type memberType,
            Type recordType,
            DataType dataType,
            Exception innerException)
            : base($"Conversion failed from Sqlite.{dataType} to {memberType.Name} for {recordType.Name}.{memberName}.", innerException)
        {
            MemberName = memberName;
            MemberType = memberType;
            RecordType = recordType;
            DataType = dataType.ToString();
        }
    }
}
