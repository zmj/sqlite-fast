using System;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using (new TestTable("create table t (x int)"))
            {
            }
        }
    }
}
