using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeopleOnDuty.Migrations.Archive
{
    /// <inheritdoc />
    public partial class InitialArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "archive");

            migrationBuilder.CreateTable(
                name: "ArchiveTasks",
                schema: "archive",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Reward = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTasks_AuthorId",
                schema: "archive",
                table: "ArchiveTasks",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTasks_OriginalTaskId",
                schema: "archive",
                table: "ArchiveTasks",
                column: "OriginalTaskId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveTasks",
                schema: "archive");
        }
    }
}
