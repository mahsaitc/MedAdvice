using System;
using MedAdvice.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MedAdvice.Tests.Infrastructure
{
    /// A throwaway database per test, built from the application's own model.
    ///
    /// SQLite rather than the EF in-memory provider: the user deletion path opens a real
    /// transaction, which the in-memory provider cannot honour, and relational constraints
    /// are part of what these tests are checking. The connection is held open for the
    /// lifetime of the instance because an in-memory SQLite database only exists while a
    /// connection to it is open.
    public sealed class TestDatabase : IDisposable
    {
        readonly SqliteConnection connection;

        public TestDatabase()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            Options = new DbContextOptionsBuilder<MedAdviceDb>()
                .UseSqlite(connection)
                .Options;

            using (MedAdviceDb context = NewContext())
            {
                context.Database.EnsureCreated();
            }
        }

        public DbContextOptions<MedAdviceDb> Options { get; }

        public SqliteConnection Connection
        {
            get { return connection; }
        }

        /// A fresh context so a test can verify what was actually persisted rather than
        /// what happens to be sitting in the change tracker.
        public MedAdviceDb NewContext()
        {
            return new MedAdviceDb(Options);
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
