using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal readonly struct VersionToken
    {
        private readonly VersionTokenSource _source;
        private readonly int _version;

        public VersionToken(VersionTokenSource source)
        {
            _source = source;
            _version = source.Version;
        }

        public bool IsStale => _version != _source.Version;
    }

    internal class VersionTokenSource
    {
        public int Version;

        public VersionToken Token => new VersionToken(this);
    }
}
