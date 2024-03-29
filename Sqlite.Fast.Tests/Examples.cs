using System;
using System.IO;
using Xunit;

namespace Sqlite.Fast.Tests
{
    public class Examples
    {
#pragma warning disable CS0649
        public struct User
        {
            public uint Id;
            public string FirstName;
            public string LastName;
            public DateTimeOffset Created;
        }
#pragma warning restore CS0649

        private void OpenConnection()
        {
            string appdataPath = "./db";
            string dbFilename = "my.db";
            using (var connection = new Connection(Path.Combine(appdataPath, dbFilename)))
            {
                // make database calls
            }
        }

        private Connection UserDb()
        {
            var conn = new Connection();
            conn.CompileStatement("create table users (id int, firstname text, lastname text, created int)")
                .Execute();
            conn.CompileStatement<(uint, string, string, DateTimeOffset)>("insert into users values (@id, @fname, @lname, @cdate)")
                .Bind((1, "f", "l", DateTimeOffset.Now))
                .Execute();
            return conn;
        }

        [Theory]
        [InlineData(1)]
        public void SelectSingleUser(uint id) 
        {
            const string sql = "select id, firstname, lastname, created from users where id=@id";
            using (Connection conn = UserDb())
            using (var select = conn.CompileStatement<User, uint>(sql))
            {
                if (!select.Bind(id).Execute(out User user))
                    throw new Exception("not found");
                // return user;
            }
        }

        [Fact]
        public void Test_InsertUser()
        {
            InsertUser( new User {
                Id = 12,
                FirstName = "z",
                LastName = "j",
                Created = new DateTimeOffset(1984, 11, 24, 0, 0, 0, 0, TimeSpan.FromHours(-5)),
            });
        }

        void InsertUser(User user)
        {
            const string sql = "insert into users values (@id, @fname, @lname, @cdate)";
            using (Connection conn = UserDb())
            using (var insert = conn.CompileStatement<User>(sql))
            {
                insert.Bind(user).Execute();
            }
        }

        [Fact]
        public void Test_SelectAllUsers()
        {
            SelectAllUsers(new User[64]);
        }

        int SelectAllUsers(User[] users)
        {
            int i = 0;
            const string sql = "select id, firstname, lastname, created from users";
            using (Connection conn = UserDb())
            using (var select = conn.CompileResultStatement<User>(sql))
            {
                foreach (Row<User> row in select.Execute())
                    row.AssignTo(out users[i++]);
            }
            return i;
        }

        [Fact]
        public void ConverterTypes() 
        {
            ParameterConverter.Builder<User>()
                // customize parameter mappings
                .Compile();

            ResultConverter.Builder<User>()
                // customize result mappings
                .Compile();
        }
    }
}