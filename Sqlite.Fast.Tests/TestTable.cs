using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast.Tests
{
    internal class TestTable : IDisposable
    {
        public readonly Connection Connection;

        public TestTable(string createTableSql)
        {
            Connection = new Connection();
            try
            {
                using (var createTable = Connection.CompileStatement(createTableSql)) 
                {
                    createTable.Execute();
                }
            }
            catch
            {
                Connection.Dispose();
                throw;
            }
        }

        public Statement Stmt(string sql) => Connection.CompileStatement(sql);
        public Statement<T> Stmt<T>(string sql) => Connection.CompileStatement<T>(sql);
        public Statement<T> Stmt<T>(string sql, ParameterConverter<T> converter) => Connection.CompileStatement(sql, converter);
        public ResultStatement<T> RStmt<T>(string sql) => Connection.CompileResultStatement<T>(sql);
        public ResultStatement<T> RStmt<T>(string sql, ResultConverter<T> converter) => Connection.CompileStatement(sql, converter);
        public ResultStatement<T> Stmt<T>(string sql, ResultConverter<T> converter) => Connection.CompileStatement(sql, converter);
        public Statement<T, U> Stmt<T, U>(string sql) => Connection.CompileStatement<T, U>(sql);
        public Statement<T, U> Stmt<T, U>(string sql, ResultConverter<T> rc, ParameterConverter<U> pc)
            => Connection.CompileStatement(sql, rc, pc);

        public void Dispose() => Connection.Dispose();
    }
}
