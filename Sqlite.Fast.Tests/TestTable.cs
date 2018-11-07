using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
    internal class TestTable : IDisposable
    {
        public Connection Connection { get; }

        public TestTable(string createTableSql)
        {
            Connection = Connection.Open(":memory:");
            try
            {
                Connection.NewStatement(createTableSql).Execute();
            }
            catch
            {
                Connection.Dispose();
                throw;
            }
        }

        public void Dispose() => Connection.Dispose();
    }
}
