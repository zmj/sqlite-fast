using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite.Fast
{
    /// <summary>
    /// Connection wraps a SQLite database connection handle. 
    /// Create a Connection instance, use it for the duration of the application session, then dispose it.
    /// Connection instances are thread-safe.
    /// </summary>
    public sealed class Connection : IDisposable
    {
        static Connection() => SQLitePCL.Batteries_V2.Init();

        private readonly IntPtr _connnection;

        private bool _disposed = false;
        
        /// <summary>
        /// Opens a database connection handle.
        /// </summary>
        /// <param name="dbFilePath">The full or relative path to a database file, or :memory: for an in-memory database.</param>
        public Connection(string dbFilePath)
        {
            // Marshal screws up the utf16->utf8 reencode, so do it in managed
            byte[] utf8Path = Encoding.UTF8.GetBytes(dbFilePath);
            Sqlite.Result r = Sqlite.Open(ref MemoryMarshal.GetReference(utf8Path.AsSpan()), out IntPtr conn);
            _connnection = conn;

            try { r.ThrowIfNotOK(nameof(Sqlite.Open)); }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Prepares a statement that has no parameters or result rows.
        /// </summary>
        public Statement CompileStatement(string sql)
        {
            CheckDisposed();
            Sqlite.Prepare16V2(
                _connnection,
                sql: ref MemoryMarshal.GetReference(sql.AsSpan()),
                sqlByteCount: -1,
                out IntPtr stmt,
                out _)
                .ThrowIfNotOK(nameof(Sqlite.Prepare16V2));
            return new Statement(stmt);
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters.
        /// </summary>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        public Statement<TParams> CompileStatement<TParams>(string sql)
        {
            var converter = ParameterConverter.Default<TParams>();
            return CompileStatement(sql, converter);
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters.
        /// </summary>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        /// <param name="sql"></param>
        /// <param name="converter">A custom converter from the parameter type to SQLite values.</param>
        public Statement<TParams> CompileStatement<TParams>(string sql, ParameterConverter<TParams> converter)
        {
            return new Statement<TParams>(CompileStatement(sql), converter);
        }
        
        /// <summary>
        /// Prepares a statement that has one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        public ResultStatement<TResult> CompileResultStatement<TResult>(string sql)
        {
            var converter = ResultConverter.Default<TResult>();
            return CompileStatement(sql, converter);
        }

        /// <summary>
        /// Prepares a statement that has one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        /// <param name="sql"></param>
        /// <param name="converter">A custom converter from SQLite values to the result type.</param>
        public ResultStatement<TResult> CompileStatement<TResult>(string sql, ResultConverter<TResult> converter)
        {
            return new ResultStatement<TResult>(CompileStatement(sql), converter);
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters and one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql)
        {
            var resultConverter = ResultConverter.Default<TResult>();
            var parameterConverter = ParameterConverter.Default<TParams>();
            return CompileStatement(sql, resultConverter, parameterConverter);
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters and one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        /// <param name="sql"></param>
        /// <param name="converter">A custom converter from the parameter type to SQLite values.</param>
        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql, ParameterConverter<TParams> converter)
        {
            return CompileStatement(sql, ResultConverter.Default<TResult>(), converter);
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters and one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        /// <param name="sql"></param>
        /// <param name="converter">A custom converter from SQLite values to the result type.</param>
        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql, ResultConverter<TResult> converter)
        {
            return CompileStatement(sql, converter, ParameterConverter.Default<TParams>());
        }

        /// <summary>
        /// Prepares a statement that has one or more parameters and one or more result rows.
        /// </summary>
        /// <typeparam name="TResult">The type of this statement's result row(s).</typeparam>
        /// <typeparam name="TParams">The type of this statement's parameter(s).</typeparam>
        /// <param name="sql"></param>
        /// <param name="resultConverter">A custom converter from SQLite values to the result type.</param>
        /// <param name="parameterConverter">A custom converter from the parameter type to SQLite values.</param>
        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(
            string sql,
            ResultConverter<TResult> resultConverter,
            ParameterConverter<TParams> parameterConverter)
        {
            Statement statement = CompileStatement(sql);
            return new Statement<TResult, TParams>(
                new ResultStatement<TResult>(statement, resultConverter),
                new Statement<TParams>(statement, parameterConverter));
        }

        /// <summary>
        /// Attempts to execute a WAL checkpoint in the specified mode.
        /// Returns true if the checkpoint succeeds, false if the checkpoint is
        /// unable to begin within the configured busy timeout.
        /// </summary>
        public bool TryCheckpoint(Sqlite.CheckpointMode mode)
        {
            CheckDisposed();
            byte attachedTableName = default;
            Sqlite.Result r = Sqlite.WalCheckpointV2(_connnection, ref attachedTableName, mode, out _, out _);
            if (r == Sqlite.Result.Busy || r == Sqlite.Result.Locked)
            {
                return false;
            }
            r.ThrowIfNotOK(nameof(Sqlite.WalCheckpointV2));
            return true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        /// <summary>
        /// Closes the database connection handle. If Dispose is not called,
        /// the database connection handle will be closed by the finalizer thread.
        /// </summary>
        public void Dispose() => Dispose(disposing: true);

        /// <summary>
        /// Closes the database connection handle.
        /// </summary>
        ~Connection() => Dispose(disposing: false);

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            Sqlite.Result r = Sqlite.CloseV2(_connnection);
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            _disposed = true;
            r.ThrowIfNotOK(nameof(Sqlite.CloseV2));
        }
    }
}