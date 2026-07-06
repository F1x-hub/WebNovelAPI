using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicWebNovelAPI.Migrations
{
    /// <inheritdoc />
    public partial class OAuthSessionLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if OAuthLogs table exists before trying to drop it
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    "IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OAuthLogs') " +
                    "DROP TABLE [OAuthLogs]");
            }

            migrationBuilder.CreateTable(
                name: "OAuthSessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    FlowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthSessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "OAuthSessionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestParams = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    ExtraData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthSessionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthSessionLogs_OAuthSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "OAuthSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OAuthSessionLogs_SessionId",
                table: "OAuthSessionLogs",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthSessionLogs");

            migrationBuilder.DropTable(
                name: "OAuthSessions");

                        // We're not recreating the OAuthLogs table on rollback since it didn't exist in the original database
        }
    }
}
