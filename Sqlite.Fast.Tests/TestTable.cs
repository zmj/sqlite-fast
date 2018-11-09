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
            _conn = Connection.Open(":memory:");
            try
            {
                using (var createTable = _conn.NewStatement(createTableSql)) 
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

        public Statement Stmt(string sql) => _conn.NewStatement(sql);

        public void Dispose() => _conn.Dispose();
    }
}
