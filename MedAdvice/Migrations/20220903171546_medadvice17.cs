using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "Products",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "CollaborationDate",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollaborationDate",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Products",
                newName: "name");
        }
    }
}
