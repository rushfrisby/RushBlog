using Microsoft.EntityFrameworkCore.Migrations;

namespace RushBlog.DataAccess.Migrations
{
    public partial class Addtemplatesectionseeddata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TemplateSections",
                columns: new[] { "Id", "Content", "Name" },
                values: new object[] { 1, "", "Header" });

            migrationBuilder.InsertData(
                table: "TemplateSections",
                columns: new[] { "Id", "Content", "Name" },
                values: new object[] { 2, "", "Footer" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
