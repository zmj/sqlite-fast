using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class EncodingTests
    {
        [Fact]
        public void NonAscii_CompileStatement()
        {
            using (new TestTable("create table ф (int x)"))
            {
            }
        }
    }
}
