using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AziendaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Aziende",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Indirizzo = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aziende", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Prodotti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AziendaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prodotti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prodotti_Aziende_AziendaId",
                        column: x => x.AziendaId,
                        principalTable: "Aziende",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sviluppatori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AziendaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(40)", nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sviluppatori", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sviluppatori_Aziende_AziendaId",
                        column: x => x.AziendaId,
                        principalTable: "Aziende",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SviluppaProdotti",
                columns: table => new
                {
                    ProdottoId = table.Column<int>(type: "int", nullable: false),
                    SviluppatoreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SviluppaProdotti", x => new { x.SviluppatoreId, x.ProdottoId });
                    table.ForeignKey(
                        name: "FK_SviluppaProdotti_Prodotti_ProdottoId",
                        column: x => x.ProdottoId,
                        principalTable: "Prodotti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SviluppaProdotti_Sviluppatori_SviluppatoreId",
                        column: x => x.SviluppatoreId,
                        principalTable: "Sviluppatori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Aziende",
                columns: new[] { "Id", "Indirizzo", "Nome" },
                values: new object[,]
                {
                    { 1, "One Microsoft Way, Redmond, WA 98052, Stati Uniti", "Microsoft" },
                    { 2, "1600 Amphitheatre Pkwy, Mountain View, CA 94043, Stati Uniti", "Google" },
                    { 3, "1 Apple Park Way Cupertino, California, 95014-0642 United States", "Apple" }
                });

            migrationBuilder.InsertData(
                table: "Prodotti",
                columns: new[] { "Id", "AziendaId", "Descrizione", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "Applicazione per la gestione delle Note", "SuperNote" },
                    { 2, 1, "Applicazione per la visione di film in streaming", "My Cinema" },
                    { 3, 2, "Applicazione per il cad 3d", "SuperCad" }
                });

            migrationBuilder.InsertData(
                table: "Sviluppatori",
                columns: new[] { "Id", "AziendaId", "Cognome", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "Rossi", "Mario" },
                    { 2, 1, "Verdi", "Giulio" },
                    { 3, 2, "Bianchi", "Leonardo" }
                });

            migrationBuilder.InsertData(
                table: "SviluppaProdotti",
                columns: new[] { "ProdottoId", "SviluppatoreId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 3, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prodotti_AziendaId",
                table: "Prodotti",
                column: "AziendaId");

            migrationBuilder.CreateIndex(
                name: "IX_SviluppaProdotti_ProdottoId",
                table: "SviluppaProdotti",
                column: "ProdottoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sviluppatori_AziendaId",
                table: "Sviluppatori",
                column: "AziendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SviluppaProdotti");

            migrationBuilder.DropTable(
                name: "Prodotti");

            migrationBuilder.DropTable(
                name: "Sviluppatori");

            migrationBuilder.DropTable(
                name: "Aziende");
        }
    }
}
