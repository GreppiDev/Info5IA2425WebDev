using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTmdbIdUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TmdbId",
                table: "Registi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registi_TmdbId",
                table: "Registi",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Films_TmdbId",
                table: "Films",
                column: "TmdbId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registi_TmdbId",
                table: "Registi");

            migrationBuilder.DropIndex(
                name: "IX_Films_TmdbId",
                table: "Films");

            migrationBuilder.DropColumn(
                name: "TmdbId",
                table: "Registi");
        }
    }
}
