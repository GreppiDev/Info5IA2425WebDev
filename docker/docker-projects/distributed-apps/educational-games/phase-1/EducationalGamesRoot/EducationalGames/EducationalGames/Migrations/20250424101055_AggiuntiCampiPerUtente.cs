using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationalGames.Migrations
{
    /// <inheritdoc />
    public partial class AggiuntiCampiPerUtente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerificata",
                table: "UTENTI",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScadenzaTokenResetPassword",
                table: "UTENTI",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScadenzaTokenVerificaEmail",
                table: "UTENTI",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenResetPassword",
                table: "UTENTI",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TokenVerificaEmail",
                table: "UTENTI",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificata",
                table: "UTENTI");

            migrationBuilder.DropColumn(
                name: "ScadenzaTokenResetPassword",
                table: "UTENTI");

            migrationBuilder.DropColumn(
                name: "ScadenzaTokenVerificaEmail",
                table: "UTENTI");

            migrationBuilder.DropColumn(
                name: "TokenResetPassword",
                table: "UTENTI");

            migrationBuilder.DropColumn(
                name: "TokenVerificaEmail",
                table: "UTENTI");
        }
    }
}
