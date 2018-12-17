using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
    internal class TestTable : IDisposable
    {
        private readonly Connection _conn;

        public TestTable(string createTableSql)
        {
            _conn = new Connection(":memory:");
            try
            {
                using (var createTable = _conn.CompileStatement(createTableSql)) 
                {
                    createTable.Execute();
                }
            }
            catch
            {
                _conn.Dispose();
                throw;
            }
        }

        public Statement Stmt(string sql) => _conn.CompileStatement(sql);
        public Statement<T> Stmt<T>(string sql) => _conn.CompileStatement<T>(sql);
        public Statement<T> Stmt<T>(string sql, ParameterConverter<T> converter) => _conn.CompileStatement(sql, converter);
        public ResultStatement<T> RStmt<T>(string sql) => _conn.CompileResultStatement<T>(sql);
        public ResultStatement<T> Stmt<T>(string sql, ResultConverter<T> converter) => _conn.CompileStatement(sql, converter);
        public Statement<T, U> Stmt<T, U>(string sql) => _conn.CompileStatement<T, U>(sql);
        public Statement<T, U> Stmt<T, U>(string sql, ParameterConverter<T> pc, ResultConverter<U> rc)
            => _conn.CompileStatement(sql, pc, rc);

        public void Dispose() => _conn.Dispose();
    }
}
