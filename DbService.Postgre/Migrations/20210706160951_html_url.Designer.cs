﻿// <auto-generated />
using ClassScraper.DbLayer.PostgreService;
using ClassScraper.DomainObjects.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DbLayer.PostgreService.Migrations
{
    [DbContext(typeof(PostgreGithubContext))]
    [Migration("20210706160951_html_url")]
    partial class html_url
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasPostgresEnum(null, "term_type", new[] { "undefined", "class", "interface" })
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("DbLayer.PostgreService.Models.PostgreFileEntity", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("HtmlUrl")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("OwnerName")
                        .HasColumnType("text");

                    b.Property<string>("Path")
                        .HasColumnType("text");

                    b.Property<string>("RepositoryName")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("DbLayer.PostgreService.Models.PostgreTermEntity", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PostgreFileEntityId")
                        .HasColumnType("text");

                    b.Property<TermType>("TermType")
                        .HasColumnType("term_type")
                        .HasColumnName("Type");

                    b.HasKey("ID");

                    b.HasIndex("PostgreFileEntityId");

                    b.ToTable("Terms");
                });

            modelBuilder.Entity("DbLayer.PostgreService.Models.PostgreTermEntity", b =>
                {
                    b.HasOne("DbLayer.PostgreService.Models.PostgreFileEntity", "PostgreFileEntity")
                        .WithMany()
                        .HasForeignKey("PostgreFileEntityId");

                    b.Navigation("PostgreFileEntity");
                });
#pragma warning restore 612, 618
        }
    }
}
