﻿// <auto-generated />
using AziendaAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AziendaAPI.Migrations
{
    [DbContext(typeof(AziendaDbContext))]
    [Migration("20250414173133_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("AziendaAPI.Model.Azienda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Indirizzo")
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Aziende");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Indirizzo = "One Microsoft Way, Redmond, WA 98052, Stati Uniti",
                            Nome = "Microsoft"
                        },
                        new
                        {
                            Id = 2,
                            Indirizzo = "1600 Amphitheatre Pkwy, Mountain View, CA 94043, Stati Uniti",
                            Nome = "Google"
                        },
                        new
                        {
                            Id = 3,
                            Indirizzo = "1 Apple Park Way Cupertino, California, 95014-0642 United States",
                            Nome = "Apple"
                        });
                });

            modelBuilder.Entity("AziendaAPI.Model.Prodotto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AziendaId")
                        .HasColumnType("int");

                    b.Property<string>("Descrizione")
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("AziendaId");

                    b.ToTable("Prodotti");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AziendaId = 1,
                            Descrizione = "Applicazione per la gestione delle Note",
                            Nome = "SuperNote"
                        },
                        new
                        {
                            Id = 2,
                            AziendaId = 1,
                            Descrizione = "Applicazione per la visione di film in streaming",
                            Nome = "My Cinema"
                        },
                        new
                        {
                            Id = 3,
                            AziendaId = 2,
                            Descrizione = "Applicazione per il cad 3d",
                            Nome = "SuperCad"
                        });
                });

            modelBuilder.Entity("AziendaAPI.Model.SviluppaProdotto", b =>
                {
                    b.Property<int>("SviluppatoreId")
                        .HasColumnType("int");

                    b.Property<int>("ProdottoId")
                        .HasColumnType("int");

                    b.HasKey("SviluppatoreId", "ProdottoId");

                    b.HasIndex("ProdottoId");

                    b.ToTable("SviluppaProdotti");

                    b.HasData(
                        new
                        {
                            SviluppatoreId = 1,
                            ProdottoId = 1
                        },
                        new
                        {
                            SviluppatoreId = 2,
                            ProdottoId = 1
                        },
                        new
                        {
                            SviluppatoreId = 3,
                            ProdottoId = 3
                        });
                });

            modelBuilder.Entity("AziendaAPI.Model.Sviluppatore", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AziendaId")
                        .HasColumnType("int");

                    b.Property<string>("Cognome")
                        .IsRequired()
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("Id");

                    b.HasIndex("AziendaId");

                    b.ToTable("Sviluppatori");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AziendaId = 1,
                            Cognome = "Rossi",
                            Nome = "Mario"
                        },
                        new
                        {
                            Id = 2,
                            AziendaId = 1,
                            Cognome = "Verdi",
                            Nome = "Giulio"
                        },
                        new
                        {
                            Id = 3,
                            AziendaId = 2,
                            Cognome = "Bianchi",
                            Nome = "Leonardo"
                        });
                });

            modelBuilder.Entity("AziendaAPI.Model.Prodotto", b =>
                {
                    b.HasOne("AziendaAPI.Model.Azienda", "Azienda")
                        .WithMany("Prodotti")
                        .HasForeignKey("AziendaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Azienda");
                });

            modelBuilder.Entity("AziendaAPI.Model.SviluppaProdotto", b =>
                {
                    b.HasOne("AziendaAPI.Model.Prodotto", "Prodotto")
                        .WithMany("SviluppaProdotti")
                        .HasForeignKey("ProdottoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AziendaAPI.Model.Sviluppatore", "Sviluppatore")
                        .WithMany("SviluppaProdotti")
                        .HasForeignKey("SviluppatoreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Prodotto");

                    b.Navigation("Sviluppatore");
                });

            modelBuilder.Entity("AziendaAPI.Model.Sviluppatore", b =>
                {
                    b.HasOne("AziendaAPI.Model.Azienda", "Azienda")
                        .WithMany("Sviluppatori")
                        .HasForeignKey("AziendaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Azienda");
                });

            modelBuilder.Entity("AziendaAPI.Model.Azienda", b =>
                {
                    b.Navigation("Prodotti");

                    b.Navigation("Sviluppatori");
                });

            modelBuilder.Entity("AziendaAPI.Model.Prodotto", b =>
                {
                    b.Navigation("SviluppaProdotti");
                });

            modelBuilder.Entity("AziendaAPI.Model.Sviluppatore", b =>
                {
                    b.Navigation("SviluppaProdotti");
                });
#pragma warning restore 612, 618
        }
    }
}
