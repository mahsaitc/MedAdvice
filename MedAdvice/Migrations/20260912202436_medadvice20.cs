using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAdvice.Migrations
{
    /// <inheritdoc />
    public partial class medadvice20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "img",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "HeroImage",
                table: "HomepageContents");

            migrationBuilder.DropColumn(
                name: "DrSpacialityPicture",
                table: "DoctorSpacialities");

            migrationBuilder.DropColumn(
                name: "DrProfileImage",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Doctorimg",
                table: "DoctorImages");

            migrationBuilder.DropColumn(
                name: "BlogHeaderImage",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "Blogimg",
                table: "BlogImages");

            migrationBuilder.DropColumn(
                name: "BlogCategoryPicture",
                table: "blogCategories");

            migrationBuilder.DropColumn(
                name: "AdviceHeaderImage",
                table: "Advices");

            migrationBuilder.DropColumn(
                name: "Adviceimg",
                table: "AdviceImages");

            migrationBuilder.DropColumn(
                name: "AdviceCategoryPicture",
                table: "adviceCategories");

            migrationBuilder.AddColumn<string>(
                name: "imgPath",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImagePath",
                table: "HomepageContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrSpacialityPicturePath",
                table: "DoctorSpacialities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrProfileImagePath",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorimgPath",
                table: "DoctorImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlogHeaderImagePath",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlogimgPath",
                table: "BlogImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlogCategoryPicturePath",
                table: "blogCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdviceHeaderImagePath",
                table: "Advices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdviceimgPath",
                table: "AdviceImages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdviceCategoryPicturePath",
                table: "adviceCategories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imgPath",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "HeroImagePath",
                table: "HomepageContents");

            migrationBuilder.DropColumn(
                name: "DrSpacialityPicturePath",
                table: "DoctorSpacialities");

            migrationBuilder.DropColumn(
                name: "DrProfileImagePath",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "DoctorimgPath",
                table: "DoctorImages");

            migrationBuilder.DropColumn(
                name: "BlogHeaderImagePath",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "BlogimgPath",
                table: "BlogImages");

            migrationBuilder.DropColumn(
                name: "BlogCategoryPicturePath",
                table: "blogCategories");

            migrationBuilder.DropColumn(
                name: "AdviceHeaderImagePath",
                table: "Advices");

            migrationBuilder.DropColumn(
                name: "AdviceimgPath",
                table: "AdviceImages");

            migrationBuilder.DropColumn(
                name: "AdviceCategoryPicturePath",
                table: "adviceCategories");

            migrationBuilder.AddColumn<byte[]>(
                name: "img",
                table: "ProductImages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "HeroImage",
                table: "HomepageContents",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "DrSpacialityPicture",
                table: "DoctorSpacialities",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "DrProfileImage",
                table: "Doctors",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Doctorimg",
                table: "DoctorImages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BlogHeaderImage",
                table: "Blogs",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Blogimg",
                table: "BlogImages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BlogCategoryPicture",
                table: "blogCategories",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AdviceHeaderImage",
                table: "Advices",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Adviceimg",
                table: "AdviceImages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AdviceCategoryPicture",
                table: "adviceCategories",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
