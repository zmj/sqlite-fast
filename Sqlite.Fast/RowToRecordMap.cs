using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public class RowToRecordMap<TRecord>
    {
        internal IColumnToFieldMap<TRecord>[] ColumnMaps { get; } 
    }
}
