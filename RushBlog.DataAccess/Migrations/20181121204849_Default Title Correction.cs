using Microsoft.EntityFrameworkCore.Migrations;

namespace RushBlog.DataAccess.Migrations
{
    public partial class DefaultTitleCorrection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Defult Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Default Title");
        }
    }
}
