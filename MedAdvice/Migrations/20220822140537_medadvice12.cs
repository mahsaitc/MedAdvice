using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adviceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdviceCategoryname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceCategoryParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adviceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_adviceCategories_adviceCategories_AdviceCategoryParentId",
                        column: x => x.AdviceCategoryParentId,
                        principalTable: "adviceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Advices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdviceTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceBriefText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdviceCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advices_adviceCategories_AdviceCategoryId",
                        column: x => x.AdviceCategoryId,
                        principalTable: "adviceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdviceImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdviceImageTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adviceimg = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AdviceId = table.Column<int>(type: "int", nullable: false),
                    AdviceCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdviceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdviceImages_Advices_AdviceCategoryId",
                        column: x => x.AdviceCategoryId,
                        principalTable: "Advices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceImages_Advices_AdviceId",
                        column: x => x.AdviceId,
                        principalTable: "Advices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adviceCategories_AdviceCategoryParentId",
                table: "adviceCategories",
                column: "AdviceCategoryParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceImages_AdviceCategoryId",
                table: "AdviceImages",
                column: "AdviceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceImages_AdviceId",
                table: "AdviceImages",
                column: "AdviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Advices_AdviceCategoryId",
                table: "Advices",
                column: "AdviceCategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdviceImages");

            migrationBuilder.DropTable(
                name: "Advices");

            migrationBuilder.DropTable(
                name: "adviceCategories");
        }
    }
}
