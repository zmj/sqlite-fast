using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    public readonly struct Row<TRecord>
    {
        private readonly Row _row;
        private readonly RowToRecordMap<TRecord> _map;

        internal Row(Row row, RowToRecordMap<TRecord> map)
        {
            _row = row;
            _map = map;
        }

        public void AssignTo(ref TRecord record)
        {
            foreach (Column col in _row.Columns)
            {
                IColumnToFieldMap<TRecord> colMap = _map.ColumnMaps[col.Index]; // length checked at Execute
                DataType dataType = col.GetDataType();
                switch (dataType)
                {
                    case DataType.Integer:
                        colMap.AssignInteger(ref record, col.GetIntegerData());
                        break;
                    case DataType.Float:
                        colMap.AssignFloat(ref record, col.GetFloatData());
                        break;
                    case DataType.Text:
                        colMap.AssignText(ref record, col.GetTextData());
                        break;
                    case DataType.Blob:
                        colMap.AssignBlob(ref record, col.GetBlobData());
                        break;
                    case DataType.Null:
                        colMap.AssignNull(ref record);
                        break;
                    default:
                        throw new Exception($"Unknown SQLite data type {dataType}");
                }
            }
        }
    }

    public readonly struct Rows<TRecord> : IEnumerable<Row<TRecord>>
    {
        private readonly Rows _rows;
        private readonly RowToRecordMap<TRecord> _map;

        internal Rows(Rows rows, RowToRecordMap<TRecord> map)
        {
            _rows = rows;
            _map = map;
        }

        public Enumerator GetEnumerator() => new Enumerator(_rows.GetEnumerator(), _map);
        IEnumerator<Row<TRecord>> IEnumerable<Row<TRecord>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<Row<TRecord>>
        {
            private readonly RowToRecordMap<TRecord> _map;

            private Rows.Enumerator _rowEnumerator;

            internal Enumerator(Rows.Enumerator rowEnumerator, RowToRecordMap<TRecord> map)
            {
                _rowEnumerator = rowEnumerator;
                _map = map;
                Current = default;
            }

            public Row<TRecord> Current { get; private set; }
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (!_rowEnumerator.MoveNext())
                {
                    Current = default;
                    return false;
                }
                Current = new Row<TRecord>(_rowEnumerator.Current, _map);
                return true;
            }

            public void Reset()
            {
                Current = default;
                _rowEnumerator.Reset();
            }

            public void Dispose()
            {
                _rowEnumerator = default;
            }
        }
    }
}
