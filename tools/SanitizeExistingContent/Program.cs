using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedAdvice.Services;
using Microsoft.Data.SqlClient;

namespace MedAdvice.Tools.SanitizeExistingContent
{
    /// One-time cleanup for rich-text columns written before inbound sanitization existed.
    /// Reports first; only writes when --apply is passed.
    class Program
    {
        class Target
        {
            public string Table;
            public string Column;
            public Target(string table, string column)
            {
                Table = table;
                Column = column;
            }
        }

        static readonly List<Target> Targets = new List<Target>
        {
            new Target("Advices", "AdviceText"),
            new Target("Blogs", "BlogText"),
            new Target("Doctors", "DrDetails"),
        };

        static async Task<int> Main(string[] args)
        {
            bool apply = Array.IndexOf(args, "--apply") >= 0;
            string connectionString = ReadConnectionString(args);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine("No connection string.");
                Console.Error.WriteLine("  --connection \"<value>\"  or  set ConnectionStrings__MedAdviceDbConnection");
                return 2;
            }

            Console.WriteLine(apply
                ? "MODE: APPLY - rows will be updated."
                : "MODE: DRY RUN - nothing will be written. Pass --apply to commit.");
            Console.WriteLine();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    int grandTotal = 0;
                    int grandChanged = 0;

                    foreach (Target target in Targets)
                    {
                        (int total, int changed) = await ProcessAsync(connection, target, apply);
                        grandTotal += total;
                        grandChanged += changed;
                        Console.WriteLine($"  {target.Table}.{target.Column}: {total} rows, {changed} would change");
                    }

                    Console.WriteLine();
                    Console.WriteLine($"TOTAL: {grandTotal} rows scanned, {grandChanged} {(apply ? "updated" : "would change")}.");
                }
                return 0;
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine($"Database error: {ex.Message}");
                return 1;
            }
        }

        static async Task<(int total, int changed)> ProcessAsync(SqlConnection connection, Target target, bool apply)
        {
            Dictionary<int, string> updates = new Dictionary<int, string>();
            int total = 0;

            using (SqlCommand read = new SqlCommand($"SELECT [Id], [{target.Column}] FROM [{target.Table}]", connection))
            using (SqlDataReader reader = await read.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    total++;
                    if (reader.IsDBNull(1))
                        continue;

                    string original = reader.GetString(1);
                    string cleaned = HtmlContentSanitizer.Sanitize(original);
                    if (cleaned != original)
                        updates[reader.GetInt32(0)] = cleaned;
                }
            }

            if (apply)
            {
                foreach (KeyValuePair<int, string> row in updates)
                {
                    using (SqlCommand write = new SqlCommand(
                        $"UPDATE [{target.Table}] SET [{target.Column}] = @value WHERE [Id] = @id", connection))
                    {
                        write.Parameters.AddWithValue("@value", row.Value);
                        write.Parameters.AddWithValue("@id", row.Key);
                        await write.ExecuteNonQueryAsync();
                    }
                }
            }

            return (total, updates.Count);
        }

        static string ReadConnectionString(string[] args)
        {
            int i = Array.IndexOf(args, "--connection");
            if (i >= 0 && i + 1 < args.Length)
                return args[i + 1];
            return Environment.GetEnvironmentVariable("ConnectionStrings__MedAdviceDbConnection");
        }
    }
}
