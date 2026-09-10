using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAdvice.Migrations
{
    /// <inheritdoc />
    public partial class medadvice19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeaturedArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: true),
                    AdviceId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedArticles_Advices_AdviceId",
                        column: x => x.AdviceId,
                        principalTable: "Advices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeaturedArticles_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HomepageContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeroTagline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeroTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeroText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeroImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AnnouncementVisible = table.Column<bool>(type: "bit", nullable: false),
                    AnnouncementText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnnouncementCtaText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnnouncementCtaUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomepageContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomepageSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomepageSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArticles_AdviceId",
                table: "FeaturedArticles",
                column: "AdviceId");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedArticles_BlogId",
                table: "FeaturedArticles",
                column: "BlogId");

            // Seeded so the page renders exactly as it did before this migration:
            // the seven existing blocks in their current order, the new featured block
            // hidden, and the hero copy carrying today's literal strings. A null
            // HeroImage falls back to the theme image already in wwwroot.
            migrationBuilder.InsertData(
                table: "HomepageContents",
                columns: new[] { "Id", "HeroTagline", "HeroTitle", "HeroText", "HeroImage",
                                 "AnnouncementVisible", "AnnouncementText", "AnnouncementCtaText", "AnnouncementCtaUrl" },
                values: new object[] { 1, "Explore us",
                    "Our doctors have different advices for every conditions",
                    "We want a happy and healthy life for you.", null,
                    false, null, null, null });

            migrationBuilder.InsertData(
                table: "HomepageSections",
                columns: new[] { "Id", "DisplayName", "SectionKey", "IsVisible", "SortOrder" },
                values: new object[,]
                {
                    { 1, "بنر بالای صفحه", "banner", true, 1 },
                    { 2, "درباره ما", "about", true, 2 },
                    { 3, "نوبت‌دهی", "appointment", true, 3 },
                    { 4, "خدمات", "services", true, 4 },
                    { 5, "چرا ما", "choice", true, 5 },
                    { 6, "بخش ویژه", "special", true, 6 },
                    { 7, "روند کار", "work", true, 7 },
                    { 8, "مطالب منتخب", "featured", false, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeaturedArticles");

            migrationBuilder.DropTable(
                name: "HomepageContents");

            migrationBuilder.DropTable(
                name: "HomepageSections");
        }
    }
}
