using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScheduledChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledCheckUser_ScheduledCheck_ScheduledChecksId",
                table: "ScheduledCheckUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledCheck",
                table: "ScheduledCheck");

            migrationBuilder.RenameTable(
                name: "ScheduledCheck",
                newName: "ScheduledChecks");

            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                table: "ScheduledChecks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledChecks",
                table: "ScheduledChecks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledChecks_Domain",
                table: "ScheduledChecks",
                column: "Domain",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledCheckUser_ScheduledChecks_ScheduledChecksId",
                table: "ScheduledCheckUser",
                column: "ScheduledChecksId",
                principalTable: "ScheduledChecks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledCheckUser_ScheduledChecks_ScheduledChecksId",
                table: "ScheduledCheckUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledChecks",
                table: "ScheduledChecks");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledChecks_Domain",
                table: "ScheduledChecks");

            migrationBuilder.RenameTable(
                name: "ScheduledChecks",
                newName: "ScheduledCheck");

            migrationBuilder.AlterColumn<string>(
                name: "Domain",
                table: "ScheduledCheck",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledCheck",
                table: "ScheduledCheck",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledCheckUser_ScheduledCheck_ScheduledChecksId",
                table: "ScheduledCheckUser",
                column: "ScheduledChecksId",
                principalTable: "ScheduledCheck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
