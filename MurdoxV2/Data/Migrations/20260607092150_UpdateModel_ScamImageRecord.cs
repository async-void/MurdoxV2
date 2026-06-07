using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurdoxV2.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel_ScamImageRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScamImages_AHash_DHash_PHash",
                table: "ScamImages",
                columns: new[] { "AHash", "DHash", "PHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScamImages_AHash_DHash_PHash",
                table: "ScamImages");
        }
    }
}
