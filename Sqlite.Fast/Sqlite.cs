using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("Sqlite.Fast.Tests")]

namespace Sqlite.Fast
{
    internal static class Sqlite
    {
        private const string DllName = "sqlite3.dll";

        [DllImport(DllName, EntryPoint = "sqlite3_open", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr conn);

        [DllImport(DllName, EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result CloseV2(IntPtr conn);

        [DllImport(DllName, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PrepareV2(IntPtr conn, [MarshalAs(UnmanagedType.LPStr)] string sql, int sqlByteCount, out IntPtr stmt, out IntPtr endSql);

        [DllImport(DllName, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Finalize(IntPtr stmt);

        [DllImport(DllName, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result BindInteger(IntPtr stmt, int paramIndex, long value);

        [DllImport(DllName, EntryPoint = "sqlite3_bind_text", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result BindText(IntPtr stmt, int paramIndex, [MarshalAs(UnmanagedType.LPStr)] string value, int valueByteCount, IntPtr destructor);

        [DllImport(DllName, EntryPoint = "sqlite3_bind_text16", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result BindText16(IntPtr stmt, int paramIndex, [MarshalAs(UnmanagedType.U2)] in char value, int valueByteCount, IntPtr destructor);
        
        [DllImport(DllName, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Step(IntPtr stmt);

        [DllImport(DllName, EntryPoint = "sqlite3_reset", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Reset(IntPtr stmt);

        [DllImport(DllName, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumCount(IntPtr stmt);

        [DllImport(DllName, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        public static extern DataType ColumnType(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnBytes(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_bytes16", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnBytes16(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
        public static extern long ColumnInteger(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        public static extern double ColumnFloat(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnText(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnText16(IntPtr stmt, int colIndex);

        [DllImport(DllName, EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ColumnBlob(IntPtr stmt, int colIndex);
    }

    public enum Result
    {
        /// <summary>
        /// Successful result
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Generic error
        /// </summary>
        Error = 1,
        /// <summary>
        /// Internal logic error in SQLite
        /// </summary>
        Internal = 2,
        /// <summary>
        /// Access permission denied
        /// </summary>
        Perm = 3,
        /// <summary>
        /// Callback routine requested an abort
        /// </summary>
        Abort = 4,
        /// <summary>
        /// The database file is locked
        /// </summary>
        Busy = 5,
        /// <summary>
        /// A table in the database is locked
        /// </summary>
        Locked = 6,
        /// <summary>
        /// A malloc() failed
        /// </summary>
        NoMem = 7,
        /// <summary>
        /// Attempt to write a readonly database
        /// </summary>
        ReadOnly = 8,
        /// <summary>
        /// Operation terminated by sqlite3_interrupt()
        /// </summary>
        Interrupt = 9,
        /// <summary>
        /// Some kind of disk I/O error occurred
        /// </summary>
        IoErr = 10,
        /// <summary>
        /// The database disk image is malformed
        /// </summary>
        Corrupt = 11,
        /// <summary>
        /// Unknown opcode in sqlite3_file_control()
        /// </summary>
        NotFound = 12,
        /// <summary>
        /// Insertion failed because database is full
        /// </summary>
        Full = 13,
        /// <summary>
        /// Unable to open the database file
        /// </summary>
        CantOpen = 14,
        /// <summary>
        /// Database lock protocol error
        /// </summary>
        Protocol = 15,
        /// <summary>
        /// Internal use only
        /// </summary>
        Empty = 16,
        /// <summary>
        /// The database schema changed
        /// </summary>
        Schema = 17,
        /// <summary>
        /// String or BLOB exceeds size limit
        /// </summary>
        TooBig = 18,
        /// <summary>
        /// Abort due to constraint violation
        /// </summary>
        Constraint = 19,
        /// <summary>
        /// Data type mismatch
        /// </summary>
        Mismatch = 20,
        /// <summary>
        /// Library used incorrectly
        /// </summary>
        Misuse = 21,
        /// <summary>
        /// Uses OS features not supported on host
        /// </summary>
        NoLfs = 22,
        /// <summary>
        /// Authorization denied
        /// </summary>
        Auth = 23,
        /// <summary>
        /// Not used
        /// </summary>
        Format = 24,
        /// <summary>
        /// 2nd parameter to sqlite3_bind out of range
        /// </summary>
        Range = 25,
        /// <summary>
        /// File opened that is not a database file
        /// </summary>
        NotADb = 26,
        /// <summary>
        /// Notifications from sqlite3_log()
        /// </summary>
        Notice = 27,
        /// <summary>
        /// Warnings from sqlite3_log()
        /// </summary>
        Warning = 28,
        /// <summary>
        /// sqlite3_step() has another row ready
        /// </summary>
        Row = 100,
        /// <summary>
        /// sqlite3_step() has finished executing
        /// </summary>
        Done = 101,
    }

    internal enum DataType
    {
        Integer = 1,
        Float = 2,
        Text = 3,
        Blob = 4,
        Null = 5,
    }
}
