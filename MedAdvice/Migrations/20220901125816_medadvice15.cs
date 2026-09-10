using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MedAdvice.Migrations
{
    public partial class medadvice15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blogCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogCategoryname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogCategoryParentId = table.Column<int>(type: "int", nullable: true),
                    BlogCategoryPicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_blogCategories_blogCategories_BlogCategoryParentId",
                        column: x => x.BlogCategoryParentId,
                        principalTable: "blogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpacialities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpacialityTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrSpacialityParentId = table.Column<int>(type: "int", nullable: true),
                    DrSpacialityPicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpacialities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSpacialities_DoctorSpacialities_DrSpacialityParentId",
                        column: x => x.DrSpacialityParentId,
                        principalTable: "DoctorSpacialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogBriefText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlogCategoryId = table.Column<int>(type: "int", nullable: false),
                    BlogHeaderImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_blogCategories_BlogCategoryId",
                        column: x => x.BlogCategoryId,
                        principalTable: "blogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalCouncilNo = table.Column<int>(type: "int", nullable: false),
                    DrBirthDate = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamillyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrServiceLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobilenumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstagramId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwitterId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAdress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrBriefIntroduction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrSpacialityId = table.Column<int>(type: "int", nullable: false),
                    DrProfileImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_DoctorSpacialities_DrSpacialityId",
                        column: x => x.DrSpacialityId,
                        principalTable: "DoctorSpacialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogImageTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Blogimg = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BlogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogImages_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorImageTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Doctorimg = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DoctorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorImages_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blogCategories_BlogCategoryParentId",
                table: "blogCategories",
                column: "BlogCategoryParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogImages_BlogId",
                table: "BlogImages",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_BlogCategoryId",
                table: "Blogs",
                column: "BlogCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorImages_DoctorId",
                table: "DoctorImages",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_DrSpacialityId",
                table: "Doctors",
                column: "DrSpacialityId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpacialities_DrSpacialityParentId",
                table: "DoctorSpacialities",
                column: "DrSpacialityParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogImages");

            migrationBuilder.DropTable(
                name: "DoctorImages");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "blogCategories");

            migrationBuilder.DropTable(
                name: "DoctorSpacialities");
        }
    }
}
