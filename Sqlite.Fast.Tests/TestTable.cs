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
        public Statement<T> Stmt<T>(string sql, RecordConverter<T> converter) => _conn.CompileStatement(sql, converter);

        public void Dispose() => _conn.Dispose();
    }
}
