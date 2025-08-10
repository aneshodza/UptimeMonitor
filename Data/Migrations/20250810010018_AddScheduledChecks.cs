using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UptimeMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledCheck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledCheckUser",
                columns: table => new
                {
                    ScheduledChecksId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledCheckUser", x => new { x.ScheduledChecksId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ScheduledCheckUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledCheckUser_ScheduledCheck_ScheduledChecksId",
                        column: x => x.ScheduledChecksId,
                        principalTable: "ScheduledCheck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledCheckUser_UsersId",
                table: "ScheduledCheckUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledCheckUser");

            migrationBuilder.DropTable(
                name: "ScheduledCheck");
        }
    }
}
