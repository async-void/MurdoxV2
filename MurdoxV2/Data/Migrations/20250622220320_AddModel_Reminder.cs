using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurdoxV2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModel_Reminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminder_Members_MemberId",
                table: "Reminder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder");

            migrationBuilder.RenameTable(
                name: "Reminder",
                newName: "Reminders");

            migrationBuilder.RenameIndex(
                name: "IX_Reminder_MemberId",
                table: "Reminders",
                newName: "IX_Reminders_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Members_MemberId",
                table: "Reminders",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Members_MemberId",
                table: "Reminders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reminders",
                table: "Reminders");

            migrationBuilder.RenameTable(
                name: "Reminders",
                newName: "Reminder");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_MemberId",
                table: "Reminder",
                newName: "IX_Reminder_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reminder",
                table: "Reminder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminder_Members_MemberId",
                table: "Reminder",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
