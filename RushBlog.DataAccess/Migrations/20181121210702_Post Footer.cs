using Microsoft.EntityFrameworkCore.Migrations;

namespace RushBlog.DataAccess.Migrations
{
    public partial class PostFooter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Default Title");

            migrationBuilder.InsertData(
                table: "TemplateSections",
                columns: new[] { "Id", "Content", "Name" },
                values: new object[] { 4, "", "Post Footer" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Defult Title");
        }
    }
}
