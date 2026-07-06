using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicWebNovelAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNovelIdToChapterCommentsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NovelId",
                table: "ChapterComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE cc
                SET cc.NovelId = c.NovelId
                FROM ChapterComments cc
                JOIN Chapters c ON cc.ChapterId = c.Id
                WHERE cc.NovelId = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterComments_NovelId",
                table: "ChapterComments",
                column: "NovelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterComments_Novels_NovelId",
                table: "ChapterComments",
                column: "NovelId",
                principalTable: "Novels",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChapterComments_Novels_NovelId",
                table: "ChapterComments");

            migrationBuilder.DropIndex(
                name: "IX_ChapterComments_NovelId",
                table: "ChapterComments");

            migrationBuilder.DropColumn(
                name: "NovelId",
                table: "ChapterComments");
        }
    }
}
