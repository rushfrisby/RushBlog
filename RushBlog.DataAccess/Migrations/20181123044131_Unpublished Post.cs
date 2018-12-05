using Microsoft.EntityFrameworkCore.Migrations;

namespace RushBlog.DataAccess.Migrations
{
    public partial class UnpublishedPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TemplateSections",
                columns: new[] { "Id", "Content", "Name" },
                values: new object[] { 5, "", "Unpublished Post" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TemplateSections",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
