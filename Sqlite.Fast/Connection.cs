using SQLitePCL;
using SQLitePCL.Ugly;
using System;

namespace Sqlite.Fast
{
    /// <summary>
    /// Connection wraps a SQLite database connection handle. 
    /// Create a Connection instance, use it for the duration of the application session, then dispose it.
    /// Connection instances are thread-safe.
    /// </summary>
    public sealed class Connection : IDisposable
    {
        static Connection() => Batteries_V2.Init();

        private readonly sqlite3 _db;
        
        /// <summary>
        /// Opens a database connection handle.
        /// </summary>
        /// <param name="dbFilePath">The full or relative path to a database file.</param>
        public Connection(string dbFilePath)
        {
            dbFilePath.ThrowIfNull(nameof(dbFilePath));
            _db = ugly.open(dbFilePath);
        }

        /// <summary>
        /// Opens a database connection handle to a transient in-memory database.
        /// Any data in this database will be lost when this connection is closed.
        /// </summary>
        public Connection() : this(":memory:") { }

        /// <summary>
        /// Prepares a statement that has no parameters or result rows.
        /// </summary>
        public Statement CompileStatement(string sql)
        {
            _db.ThrowIfClosed(nameof(Connection));
            sql.ThrowIfNull(nameof(sql));
            var stmt = _db.prepare(sql);
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
            _db.ThrowIfClosed(nameof(Connection));
            converter.ThrowIfNull(nameof(ParameterConverter));
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
            _db.ThrowIfClosed(nameof(Connection));
            converter.ThrowIfNull(nameof(ResultConverter));
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
            resultConverter.ThrowIfNull(nameof(ResultConverter));
            parameterConverter.ThrowIfNull(nameof(ParameterConverter));
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
            _db.ThrowIfClosed(nameof(Connection));
            try
            {
                _db.wal_checkpoint(dbName: default, (int)mode, out _, out _);
                return true;
            }
            catch (ugly.sqlite3_exception ex)
                when (ex.errcode == raw.SQLITE_BUSY || ex.errcode == raw.SQLITE_LOCKED)
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the database connection handle. If Dispose is not called,
        /// the database connection handle will be closed by the finalizer thread.
        /// </summary>
        public void Dispose() => _db.Dispose();
    }
}