using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Data;
using MedAdvice.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedAdvice.IntegrationTests
{
    /// Group C: the 17 migrations against a real SQL Server engine. Confirmed viable during
    /// planning -- 17 migrations, 26 tables, HomepageSections seeded 8 rows (7 visible, the
    /// new "featured" block hidden), HomepageContents seeded with the hero title, and the
    /// AdviceComment/adviceComments table-name pin from Batch 5 holding.
    public class MigrationTests : IClassFixture<SqlServerFixture>
    {
        readonly SqlServerFixture fixture;

        public MigrationTests(SqlServerFixture fixture)
        {
            this.fixture = fixture;
        }

        [SkippableFact]
        public async Task All_seventeen_migrations_apply_from_empty()
        {
            Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);

            using (MedAdviceDb context = NewContext())
            {
                await context.Database.MigrateAsync();

                IEnumerable<string> applied = await context.Database.GetAppliedMigrationsAsync();
                Assert.Equal(17, applied.Count());
            }
        }

        [SkippableFact]
        public async Task Migration_history_ends_at_medadvice19()
        {
            Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);

            using (MedAdviceDb context = NewContext())
            {
                await context.Database.MigrateAsync();

                string[] applied = (await context.Database.GetAppliedMigrationsAsync())
                    .OrderBy(x => x)
                    .ToArray();

                Assert.EndsWith("_medadvice19", applied.Last());
            }
        }

        [SkippableFact]
        public async Task Seed_data_lands()
        {
            Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);

            using (MedAdviceDb context = NewContext())
            {
                await context.Database.MigrateAsync();

                List<HomepageSection> sections = await context.HomepageSections
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync();
                Assert.Equal(8, sections.Count);
                Assert.Equal(7, sections.Count(x => x.IsVisible));
                Assert.False(sections.Single(x => x.SectionKey == "featured").IsVisible);

                HomepageContent content = await context.HomepageContents.SingleAsync();
                Assert.Equal(
                    "Our doctors have different advices for every conditions",
                    content.HeroTitle);
            }
        }

        [SkippableFact]
        public async Task AdviceComment_table_is_singular_not_plural()
        {
            Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);

            using (MedAdviceDb context = NewContext())
            {
                await context.Database.MigrateAsync();

                bool singularExists = await TableExistsAsync(context, "AdviceComment");
                bool pluralExists = await TableExistsAsync(context, "adviceComments");

                Assert.True(singularExists);
                Assert.False(pluralExists);
            }
        }

        [SkippableFact]
        public async Task Migrate_is_idempotent()
        {
            Skip.IfNot(fixture.IsAvailable, fixture.SkipReason);

            using (MedAdviceDb context = NewContext())
            {
                await context.Database.MigrateAsync();
                int firstCount = (await context.Database.GetAppliedMigrationsAsync()).Count();

                // A second Migrate against an already-current database must be a no-op,
                // not a duplicate-seed error.
                await context.Database.MigrateAsync();
                int secondCount = (await context.Database.GetAppliedMigrationsAsync()).Count();
                int homepageSectionCount = await context.HomepageSections.CountAsync();

                Assert.Equal(firstCount, secondCount);
                Assert.Equal(8, homepageSectionCount);
            }
        }

        MedAdviceDb NewContext()
        {
            return new MedAdviceDb(new DbContextOptionsBuilder<MedAdviceDb>()
                .UseSqlServer(fixture.NewConnectionString())
                .Options);
        }

        static async Task<bool> TableExistsAsync(MedAdviceDb context, string tableName)
        {
            int count = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}",
                    tableName)
                .SingleAsync();
            return count > 0;
        }
    }
}
