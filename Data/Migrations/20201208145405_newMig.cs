using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class newMig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ArticleName",
                table: "Articles",
                newName: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Articles",
                newName: "ArticleName");
        }
    }
}
