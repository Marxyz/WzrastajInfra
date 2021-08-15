using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class LinkArticleReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkEntity_Articles_ArticleMetadataEntityId",
                table: "LinkEntity");

            migrationBuilder.RenameColumn(
                name: "ArticleMetadataEntityId",
                table: "LinkEntity",
                newName: "ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkEntity_ArticleMetadataEntityId",
                table: "LinkEntity",
                newName: "IX_LinkEntity_ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkEntity_Articles_ArticleId",
                table: "LinkEntity",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkEntity_Articles_ArticleId",
                table: "LinkEntity");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "LinkEntity",
                newName: "ArticleMetadataEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkEntity_ArticleId",
                table: "LinkEntity",
                newName: "IX_LinkEntity_ArticleMetadataEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkEntity_Articles_ArticleMetadataEntityId",
                table: "LinkEntity",
                column: "ArticleMetadataEntityId",
                principalTable: "Articles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
