﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Sqlite.Fast
{
    public sealed class Connection : IDisposable
    {
        private readonly IntPtr _connnection;

        private bool _disposed = false;
        
        public Connection(string dbFilePath)
        {
            // Marshal screws up the utf16->utf8 reencode, so do it in managed
            byte[] utf8Path = Encoding.UTF8.GetBytes(dbFilePath);
            Sqlite.Result r = Sqlite.Open(MemoryMarshal.GetReference(utf8Path.AsSpan()), out IntPtr conn);
            _connnection = conn;

            try { r.ThrowIfNotOK("Failed to open database connection"); }
            catch
            {
                Dispose();
                throw;
            }
        }

        public Statement CompileStatement(string sql)
        {
            CheckDisposed();
            Sqlite.PrepareV2(
                _connnection, 
                sql: MemoryMarshal.GetReference(sql.AsSpan()), 
                sqlByteCount: -1, 
                out IntPtr stmt,
                out _)
                .ThrowIfNotOK("Failed to compile sql statement");
            return new Statement(stmt);
        }

        public Statement<TParams> CompileStatement<TParams>(string sql)
        {
            var converter = ParameterConverter.Default<TParams>();
            return CompileStatement(sql, converter);
        }

        public Statement<TParams> CompileStatement<TParams>(string sql, ParameterConverter<TParams> converter)
        {
            return new Statement<TParams>(CompileStatement(sql), converter);
        }
        
        public ResultStatement<TResult> CompileResultStatement<TResult>(string sql)
        {
            var converter = ResultConverter.Default<TResult>();
            return CompileStatement(sql, converter);
        }

        public ResultStatement<TResult> CompileStatement<TResult>(string sql, ResultConverter<TResult> converter)
        {
            return new ResultStatement<TResult>(CompileStatement(sql), converter);
        }

        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql)
        {
            var resultConverter = ResultConverter.Default<TResult>();
            var parameterConverter = ParameterConverter.Default<TParams>();
            return CompileStatement(sql, resultConverter, parameterConverter);
        }

        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql, ParameterConverter<TParams> converter)
        {
            return CompileStatement(sql, ResultConverter.Default<TResult>(), converter);
        }

        public Statement<TResult, TParams> CompileStatement<TResult, TParams>(string sql, ResultConverter<TResult> converter)
        {
            return CompileStatement(sql, converter, ParameterConverter.Default<TParams>());
        }

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
        /// TryCheckpoint attempts to execute a WAL checkpoint in the specified mode.
        /// Returns true if the checkpoint succeeds, false if the checkpoint is
        /// unable to begin within the configured busy timeout.
        /// Throws for non-contention errors.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool TryCheckpoint(Sqlite.CheckpointMode mode)
        {
            CheckDisposed();
            Sqlite.Result r = Sqlite.WalCheckpointV2(_connnection, default, mode, out _, out _);
            if (r == Sqlite.Result.Busy || r == Sqlite.Result.Locked)
            {
                return false;
            }
            r.ThrowIfNotOK("Unexpected checkpoint failure");
            return true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Statement));
            }
        }

        ~Connection() => Dispose(disposing: false);
        public void Dispose() => Dispose(disposing: true);

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            Sqlite.Result r = Sqlite.CloseV2(_connnection);
            _disposed = true;
            if (!disposing)
            {
                return;
            }
            GC.SuppressFinalize(this);
            r.ThrowIfNotOK("Failed to close database connection");
        }
    }
}