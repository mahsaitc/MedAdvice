using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdviceImages_Advices_AdviceCategoryId",
                table: "AdviceImages");

            migrationBuilder.DropIndex(
                name: "IX_AdviceImages_AdviceCategoryId",
                table: "AdviceImages");

            migrationBuilder.DropColumn(
                name: "AdviceCategoryId",
                table: "AdviceImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "AdviceHeaderImage",
                table: "Advices",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdviceHeaderImage",
                table: "Advices");

            migrationBuilder.AddColumn<int>(
                name: "AdviceCategoryId",
                table: "AdviceImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdviceImages_AdviceCategoryId",
                table: "AdviceImages",
                column: "AdviceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdviceImages_Advices_AdviceCategoryId",
                table: "AdviceImages",
                column: "AdviceCategoryId",
                principalTable: "Advices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
