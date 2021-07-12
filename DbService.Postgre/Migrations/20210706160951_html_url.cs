using Microsoft.EntityFrameworkCore.Migrations;

namespace DbLayer.PostgreService.Migrations
{
    public partial class html_url : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlUrl",
                table: "Files",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlUrl",
                table: "Files");
        }
    }
}
