using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurdoxV2.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel_ServerMember_02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastMessageTimestamp",
                table: "Members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMessageTimestamp",
                table: "Members");
        }
    }
}
