using Microsoft.EntityFrameworkCore.Migrations;

namespace DbLayer.PostgreService.Migrations
{
    public partial class term : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:term_type", "undefined,class,interface,enum,property,field")
                .OldAnnotation("Npgsql:Enum:term_type", "undefined,class,interface");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:term_type", "undefined,class,interface")
                .OldAnnotation("Npgsql:Enum:term_type", "undefined,class,interface,enum,property,field");
        }
    }
}
