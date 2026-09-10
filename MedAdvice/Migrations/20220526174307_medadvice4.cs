using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Purchasecarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isOpen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchasecarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchasecarts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchaseCartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseCartId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchaseCartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchaseCartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchaseCartItems_Purchasecarts_PurchaseCartId",
                        column: x => x.PurchaseCartId,
                        principalTable: "Purchasecarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchaseCartItems_ProductId",
                table: "purchaseCartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_purchaseCartItems_PurchaseCartId",
                table: "purchaseCartItems",
                column: "PurchaseCartId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchasecarts_UserId",
                table: "Purchasecarts",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchaseCartItems");

            migrationBuilder.DropTable(
                name: "Purchasecarts");
        }
    }
}
