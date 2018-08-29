# SQLite-Fast

SQLite-Fast is a high performance, low allocation SQLite wrapper targeting .NET Standard 1.6.

### Use Case
 
SQLite-Fast is intended for applications with a small number of distinct SQL queries (varying by parameters) that execute many times in a typical application session.

SQLite-Fast precomputes a strongly-typed mapping from a SQL result row to a target C# type. Each query execution reuses that mapping, meaning:
* No boxing
* No reflection
* By-ref assignment

SQLite-Fast does not include an ORM or query builder. There are no plans to add one. If your application needs to dynamically construct non-parameterized queries, this library is not intended for your use case.

### Project Status

This project is a prototype under active development. Use at your own risk. Expect bugs and breaking changes.

### Comparison to SQLite.Net

In an informal benchmark selecting single records by primary key, SQLite-Fast was ~3.5x faster than SQLite.Net (140 records/ms vs 35-40 records/ms). Benchmark code:

Record type:
```
internal struct DbDirectoryEntry
{
    [SQLite.PrimaryKey]
    public uint Key { get; set; }
    public uint ParentId { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
}
```

SQLite.Net:
```
private static long QueryAll_SQLiteNet(string dbPath, int maxKey, CancellationToken ct)
{
    long recordsQueried = 0;
    using (var dbConn = new SQLite.SQLiteConnection(dbPath))
    {
        while (!ct.IsCancellationRequested)
        {
            for (int key = 1; key <= maxKey && !ct.IsCancellationRequested; key++)
            {
                var de = dbConn.Find<DbDirectoryEntry>(pk: key);
                recordsQueried++;
            }
        }
    }
    return recordsQueried;
}
```

SQLite-Fast
```
private static long QueryAll_SqliteFast(string dbPath, int maxKey, CancellationToken ct)
{
    long recordsQueried = 0;
    using (var dbConn = Connection.Open(dbPath))
    using (var statement = dbConn.NewStatement("select * from dbdirectoryentry where key=@key"))
    {
        var rowMap = RowToRecordMap.Default<DbDirectoryEntry>().Compile();
        while (!ct.IsCancellationRequested)
        {
            for (int key = 1; key <= maxKey && !ct.IsCancellationRequested; key++)
            {
                statement.Bind(parameterIndex: 0, parameterValue: key);
                DbDirectoryEntry de = default;
                foreach (var row in statement.Execute(rowMap))
                {
                    row.AssignTo(ref de);
                    recordsQueried++;
                }
            }
        }
    }
    return recordsQueried;
}
```

### Installation

This library is available on NuGet: https://www.nuget.org/packages/Sqlite.Fast/

`sqlite3.dll` is not included in this library. It's up to you to choose and include the appropriate build for your target platform.
