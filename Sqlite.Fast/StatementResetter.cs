using System;
using System.Collections.Generic;
using System.Text;

namespace Sqlite.Fast
{
    internal sealed class StatementResetter
    {
        private readonly IntPtr _statement;

        private int _version = 0;

        public StatementResetter(IntPtr statement) => _statement = statement;

        public StatementVersion CurrentVersion => new StatementVersion(_version);

        public bool ResetIfNecessary(StatementVersion lastKnownVersion)
        {
            if (lastKnownVersion != CurrentVersion)
            {
                return false;
            }
            Sqlite.Reset(_statement).ThrowIfNotOK("Failed to reset statement");
            _version++;
            return true;
        }

        public void ThrowIfReset(StatementVersion expectedVersion)
        {
            if (CurrentVersion != expectedVersion)
            {
                throw new InvalidOperationException("Result enumerator used after source statement reset");
            }
        }
    }

    internal readonly struct StatementVersion : IEquatable<StatementVersion>
    {
        public readonly int Version;
        public StatementVersion(int version) => Version = version;
        public bool Equals(StatementVersion other) => Version == other.Version;
        public static bool operator ==(StatementVersion left, StatementVersion right) => left.Equals(right);
        public static bool operator !=(StatementVersion left, StatementVersion right) => !left.Equals(right);
        public override bool Equals(object obj) => obj is StatementVersion other ? Equals(other) : false;
        public override int GetHashCode() => Version.GetHashCode();
    }
}
