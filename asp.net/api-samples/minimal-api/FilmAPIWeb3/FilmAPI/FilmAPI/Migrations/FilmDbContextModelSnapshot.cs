﻿// <auto-generated />
using System;
using FilmAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FilmAPI.Migrations
{
    [DbContext(typeof(FilmDbContext))]
    partial class FilmDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("FilmAPI.Model.Cinema", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Città")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Indirizzo")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Cinemas");
                });

            modelBuilder.Entity("FilmAPI.Model.Film", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataProduzione")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Durata")
                        .HasColumnType("int");

                    b.Property<int>("RegistaId")
                        .HasColumnType("int");

                    b.Property<string>("Titolo")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RegistaId");

                    b.HasIndex("TmdbId")
                        .IsUnique();

                    b.ToTable("Films");
                });

            modelBuilder.Entity("FilmAPI.Model.Proiezione", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CinemaId")
                        .HasColumnType("int");

                    b.Property<DateOnly>("Data")
                        .HasColumnType("date");

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.Property<TimeOnly>("Ora")
                        .HasColumnType("time(6)");

                    b.HasKey("Id");

                    b.HasIndex("CinemaId");

                    b.HasIndex("FilmId");

                    b.ToTable("Proiezioni");
                });

            modelBuilder.Entity("FilmAPI.Model.Regista", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Cognome")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nazionalità")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("TmdbId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TmdbId")
                        .IsUnique();

                    b.ToTable("Registi");
                });

            modelBuilder.Entity("FilmAPI.Model.Film", b =>
                {
                    b.HasOne("FilmAPI.Model.Regista", "Regista")
                        .WithMany("Films")
                        .HasForeignKey("RegistaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Regista");
                });

            modelBuilder.Entity("FilmAPI.Model.Proiezione", b =>
                {
                    b.HasOne("FilmAPI.Model.Cinema", "Cinema")
                        .WithMany("Proiezioni")
                        .HasForeignKey("CinemaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmAPI.Model.Film", "Film")
                        .WithMany("Proiezioni")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cinema");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FilmAPI.Model.Cinema", b =>
                {
                    b.Navigation("Proiezioni");
                });

            modelBuilder.Entity("FilmAPI.Model.Film", b =>
                {
                    b.Navigation("Proiezioni");
                });

            modelBuilder.Entity("FilmAPI.Model.Regista", b =>
                {
                    b.Navigation("Films");
                });
#pragma warning restore 612, 618
        }
    }
}
