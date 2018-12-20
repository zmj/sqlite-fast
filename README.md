# SQLite-Fast

SQLite-Fast is a high performance, low allocation SQLite wrapper targeting .NET Standard 1.6. This library is available on NuGet: https://www.nuget.org/packages/Sqlite.Fast/

This is a prototype under active development. Use at your own risk. Expect bugs and breaking changes.

### Use Case
 
SQLite-Fast is intended for applications with a small number of distinct SQL queries (varying by parameters) that execute many times in a typical application session.

SQLite-Fast precomputes a strongly-typed mapping from a SQL result row to a target C# type. Each query execution reuses that mapping, meaning:
* No reflection
* No boxing

SQLite-Fast does not include an ORM or query builder. There are no plans to add one. If your application needs to dynamically construct non-parameterized queries, this library is not intended for your use case.

### How To

Open a connection:

```
using (var connection = new Connection(Path.Combine(appdataPath, dbFilename)))
{
    // make database calls
}
```

Reuse a single connection instance for the duration of an application session, then dispose it. Connections are safe for concurrent use.

Compile a SQL statement:

Reuse compiled statements for the lifetime of the underlying database connection. Statements are not safe for concurrent use.

Example queries, assuming a User model and equivalent table:

```
private struct User
{
    public uint Id;
    public string FirstName;
    public string LastName;
    public DateTimeOffset Created;
}
```

To select a single user by id:

```
User SelectSingleUser(uint id) 
{
    const string sql = "select id, firstname, lastname, created from users where id=@id";
    using (Connection conn = UserDb())
    using (var select = conn.CompileStatement<User, uint>(sql))
    {
        User user = default;
        if (!select.Bind(id).Execute(ref user)) throw new Exception("not found");
        return user;
    }
}
```

To insert a new user:

```
void InsertUser(User user)
{
    const string sql = "insert into users values (@id, @fname, @lname, @cdate)";
    using (Connection conn = UserDb())
    using (var insert = conn.CompileStatement<User>(sql))
    {
        insert.Bind(user).Execute();
    }
}
```

To select multiple users:

```
int SelectAllUsers(User[] users)
{
    int i = 0;
    const string sql = "select id, firstname, lastname, created from users";
    using (Connection conn = UserDb())
    using (var select = conn.CompileResultStatement<User>(sql))
    {
        foreach (Row<User> row in select.Execute())
            row.AssignTo(ref users[i++]);
    }
    return i;
}
```

To customize the mapping to/from a C# type and a SQL query result or parameters, use the appropriate Converter builder, and pass the Converter to a CompileStatement call or Execute call.

```
ParameterConverter.Builder<User>()
    // customize parameter mappings
    .Compile();

ResultConverter.Builder<User>()
    // customize result mappings
    .Compile();
```

### Comparison to SQLite.Net

TODO: replace with benchmark.net comparison on in-memory table

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
