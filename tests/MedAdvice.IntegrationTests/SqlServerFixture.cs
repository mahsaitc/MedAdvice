using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// Group C needs the real engine: SQLite cannot prove a migration written against
    /// SQL Server's provider actually applies, and nothing else in this suite touches
    /// LocalDB. It ships on GitHub's windows-latest runners and on this machine.
    ///
    /// Per the team's decision this group fails loudly rather than skipping silently:
    /// with MEDADVICE_TESTS_REQUIRE_SQLSERVER=1 set, an unreachable LocalDB throws here and
    /// every test that depends on this fixture is reported failed, not skipped. Without the
    /// variable, IsAvailable is false and each test reports a skip with SkipReason, so an
    /// environment that quietly never ran these tests cannot be mistaken for one that passed
    /// them.
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        const string RequireEnvVar = "MEDADVICE_TESTS_REQUIRE_SQLSERVER";
        const string MasterConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;Connect Timeout=5";

        readonly List<string> createdDatabases = new List<string>();

        public bool IsAvailable { get; private set; }
        public string SkipReason { get; private set; }

        public async Task InitializeAsync()
        {
            bool required = Environment.GetEnvironmentVariable(RequireEnvVar) == "1";

            try
            {
                using (SqlConnection connection = new SqlConnection(MasterConnectionString))
                {
                    await connection.OpenAsync();
                }
                IsAvailable = true;
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                SkipReason = "Requires a real SQL Server (LocalDB) instance; none reachable here (" +
                    ex.Message + "). Set " + RequireEnvVar + "=1 to make this a failure instead of a skip.";

                if (required)
                {
                    throw new InvalidOperationException(
                        RequireEnvVar + "=1 is set, so Group C must run, but LocalDB could not be reached: "
                        + ex.Message, ex);
                }
            }
        }

        /// A fresh, uniquely named database, empty of even the migrations history table --
        /// what "all 17 migrations apply from empty" actually needs to prove. Every database
        /// this hands out is dropped in DisposeAsync.
        public string NewConnectionString()
        {
            string name = "MedAdviceIntegrationTests_" + Guid.NewGuid().ToString("N");
            lock (createdDatabases)
            {
                createdDatabases.Add(name);
            }
            return "Server=(localdb)\\MSSQLLocalDB;Database=" + name + ";Trusted_Connection=True;Connect Timeout=5";
        }

        public async Task DisposeAsync()
        {
            if (IsAvailable == false)
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(MasterConnectionString))
            {
                await connection.OpenAsync();
                foreach (string name in createdDatabases)
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // SINGLE_USER + ROLLBACK IMMEDIATE: drop the database even if a
                        // leaked connection is still attached to it.
                        command.CommandText =
                            "IF DB_ID('" + name + "') IS NOT NULL BEGIN " +
                            "ALTER DATABASE [" + name + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                            "DROP DATABASE [" + name + "]; END";
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
