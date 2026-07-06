using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicWebNovelAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAddedChapterColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "UserLibraries");

            migrationBuilder.AddColumn<bool>(
                name: "AddedChapter",
                table: "UserLibraries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedChapter",
                table: "UserLibraries");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "UserLibraries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
