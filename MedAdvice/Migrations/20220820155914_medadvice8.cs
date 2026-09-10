using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "moredescriptions",
                table: "Products",
                newName: "model");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "model",
                table: "Products",
                newName: "moredescriptions");
        }
    }
}
