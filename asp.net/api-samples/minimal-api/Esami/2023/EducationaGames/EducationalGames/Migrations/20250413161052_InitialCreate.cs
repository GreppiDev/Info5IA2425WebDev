using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationalGames.Migrations
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
                name: "ARGOMENTI",
                columns: table => new
                {
                    ID_Argomento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomeArgomento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ARGOMENTI", x => x.ID_Argomento);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MATERIE",
                columns: table => new
                {
                    ID_Materia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomeMateria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MATERIE", x => x.ID_Materia);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UTENTI",
                columns: table => new
                {
                    ID_Utente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cognome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ruolo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UTENTI", x => x.ID_Utente);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VIDEOGIOCHI",
                columns: table => new
                {
                    ID_Gioco = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titolo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescrizioneBreve = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescrizioneEstesa = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxMonete = table.Column<uint>(type: "int unsigned", nullable: false),
                    Immagine1 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Immagine2 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Immagine3 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefinizioneGioco = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VIDEOGIOCHI", x => x.ID_Gioco);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CLASSI_VIRTUALI",
                columns: table => new
                {
                    ID_Classe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomeClasse = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodiceIscrizione = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_Docente = table.Column<int>(type: "int", nullable: false),
                    ID_Materia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLASSI_VIRTUALI", x => x.ID_Classe);
                    table.ForeignKey(
                        name: "FK_CLASSI_VIRTUALI_MATERIE_ID_Materia",
                        column: x => x.ID_Materia,
                        principalTable: "MATERIE",
                        principalColumn: "ID_Materia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLASSI_VIRTUALI_UTENTI_ID_Docente",
                        column: x => x.ID_Docente,
                        principalTable: "UTENTI",
                        principalColumn: "ID_Utente",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GIOCHI_ARGOMENTI",
                columns: table => new
                {
                    ID_Gioco = table.Column<int>(type: "int", nullable: false),
                    ID_Argomento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GIOCHI_ARGOMENTI", x => new { x.ID_Gioco, x.ID_Argomento });
                    table.ForeignKey(
                        name: "FK_GIOCHI_ARGOMENTI_ARGOMENTI_ID_Argomento",
                        column: x => x.ID_Argomento,
                        principalTable: "ARGOMENTI",
                        principalColumn: "ID_Argomento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GIOCHI_ARGOMENTI_VIDEOGIOCHI_ID_Gioco",
                        column: x => x.ID_Gioco,
                        principalTable: "VIDEOGIOCHI",
                        principalColumn: "ID_Gioco",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CLASSI_GIOCHI",
                columns: table => new
                {
                    ID_Classe = table.Column<int>(type: "int", nullable: false),
                    ID_Gioco = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLASSI_GIOCHI", x => new { x.ID_Classe, x.ID_Gioco });
                    table.ForeignKey(
                        name: "FK_CLASSI_GIOCHI_CLASSI_VIRTUALI_ID_Classe",
                        column: x => x.ID_Classe,
                        principalTable: "CLASSI_VIRTUALI",
                        principalColumn: "ID_Classe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CLASSI_GIOCHI_VIDEOGIOCHI_ID_Gioco",
                        column: x => x.ID_Gioco,
                        principalTable: "VIDEOGIOCHI",
                        principalColumn: "ID_Gioco",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ISCRIZIONI",
                columns: table => new
                {
                    ID_Studente = table.Column<int>(type: "int", nullable: false),
                    ID_Classe = table.Column<int>(type: "int", nullable: false),
                    DataIscrizione = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISCRIZIONI", x => new { x.ID_Studente, x.ID_Classe });
                    table.ForeignKey(
                        name: "FK_ISCRIZIONI_CLASSI_VIRTUALI_ID_Classe",
                        column: x => x.ID_Classe,
                        principalTable: "CLASSI_VIRTUALI",
                        principalColumn: "ID_Classe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ISCRIZIONI_UTENTI_ID_Studente",
                        column: x => x.ID_Studente,
                        principalTable: "UTENTI",
                        principalColumn: "ID_Utente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PROGRESSI_STUDENTI",
                columns: table => new
                {
                    ID_Studente = table.Column<int>(type: "int", nullable: false),
                    ID_Gioco = table.Column<int>(type: "int", nullable: false),
                    ID_Classe = table.Column<int>(type: "int", nullable: false),
                    MoneteRaccolte = table.Column<uint>(type: "int unsigned", nullable: false),
                    UltimoAggiornamento = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROGRESSI_STUDENTI", x => new { x.ID_Studente, x.ID_Gioco, x.ID_Classe });
                    table.ForeignKey(
                        name: "FK_PROGRESSI_STUDENTI_CLASSI_VIRTUALI_ID_Classe",
                        column: x => x.ID_Classe,
                        principalTable: "CLASSI_VIRTUALI",
                        principalColumn: "ID_Classe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROGRESSI_STUDENTI_UTENTI_ID_Studente",
                        column: x => x.ID_Studente,
                        principalTable: "UTENTI",
                        principalColumn: "ID_Utente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROGRESSI_STUDENTI_VIDEOGIOCHI_ID_Gioco",
                        column: x => x.ID_Gioco,
                        principalTable: "VIDEOGIOCHI",
                        principalColumn: "ID_Gioco",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ARGOMENTI_NomeArgomento",
                table: "ARGOMENTI",
                column: "NomeArgomento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLASSI_GIOCHI_ID_Gioco",
                table: "CLASSI_GIOCHI",
                column: "ID_Gioco");

            migrationBuilder.CreateIndex(
                name: "IX_CLASSI_VIRTUALI_CodiceIscrizione",
                table: "CLASSI_VIRTUALI",
                column: "CodiceIscrizione",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLASSI_VIRTUALI_ID_Docente_NomeClasse",
                table: "CLASSI_VIRTUALI",
                columns: new[] { "ID_Docente", "NomeClasse" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLASSI_VIRTUALI_ID_Materia",
                table: "CLASSI_VIRTUALI",
                column: "ID_Materia");

            migrationBuilder.CreateIndex(
                name: "IX_GIOCHI_ARGOMENTI_ID_Argomento",
                table: "GIOCHI_ARGOMENTI",
                column: "ID_Argomento");

            migrationBuilder.CreateIndex(
                name: "IX_ISCRIZIONI_ID_Classe",
                table: "ISCRIZIONI",
                column: "ID_Classe");

            migrationBuilder.CreateIndex(
                name: "IX_MATERIE_NomeMateria",
                table: "MATERIE",
                column: "NomeMateria",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROGRESSI_STUDENTI_ID_Classe",
                table: "PROGRESSI_STUDENTI",
                column: "ID_Classe");

            migrationBuilder.CreateIndex(
                name: "IX_PROGRESSI_STUDENTI_ID_Gioco",
                table: "PROGRESSI_STUDENTI",
                column: "ID_Gioco");

            migrationBuilder.CreateIndex(
                name: "IX_UTENTI_Email",
                table: "UTENTI",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VIDEOGIOCHI_Titolo",
                table: "VIDEOGIOCHI",
                column: "Titolo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CLASSI_GIOCHI");

            migrationBuilder.DropTable(
                name: "GIOCHI_ARGOMENTI");

            migrationBuilder.DropTable(
                name: "ISCRIZIONI");

            migrationBuilder.DropTable(
                name: "PROGRESSI_STUDENTI");

            migrationBuilder.DropTable(
                name: "ARGOMENTI");

            migrationBuilder.DropTable(
                name: "CLASSI_VIRTUALI");

            migrationBuilder.DropTable(
                name: "VIDEOGIOCHI");

            migrationBuilder.DropTable(
                name: "MATERIE");

            migrationBuilder.DropTable(
                name: "UTENTI");
        }
    }
}
