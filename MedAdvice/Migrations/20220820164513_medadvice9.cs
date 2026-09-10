using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "model",
                table: "Products",
                newName: "productmodel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "productmodel",
                table: "Products",
                newName: "model");
        }
    }
}
