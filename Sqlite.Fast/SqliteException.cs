﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class SqliteException : Exception
    {
        public Result Error { get; }

        public SqliteException(Result err, string msg)
            : base($"{msg} (SQLite error code {err})")
        {
            Error = err;
        }
    }
}