﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations.Data.Migrations
{
    [DbContext(typeof(IntegrationsDbContext))]
    partial class IntegrationsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("FriendlyName")
                        .HasColumnName("friendly_name")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnName("xml")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys");
                });

            modelBuilder.Entity("Ranger.Services.Integrations.Data.IntegrationStream", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data")
                        .HasColumnType("jsonb");

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasColumnName("event")
                        .HasColumnType("text");

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnName("inserted_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("InsertedBy")
                        .IsRequired()
                        .HasColumnName("inserted_by")
                        .HasColumnType("text");

                    b.Property<int>("IntegrationType")
                        .HasColumnName("integration_type")
                        .HasColumnType("integer");

                    b.Property<Guid>("StreamId")
                        .HasColumnName("stream_id")
                        .HasColumnType("uuid");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnName("tenant_id")
                        .HasColumnType("text");

                    b.Property<int>("Version")
                        .HasColumnName("version")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_integration_streams");

                    b.ToTable("integration_streams");
                });

            modelBuilder.Entity("Ranger.Services.Integrations.Data.IntegrationUniqueConstraint", b =>
                {
                    b.Property<Guid>("IntegrationId")
                        .HasColumnName("integration_id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(140)")
                        .HasMaxLength(140);

                    b.Property<Guid>("ProjectId")
                        .HasColumnName("project_id")
                        .HasColumnType("uuid");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnName("tenant_id")
                        .HasColumnType("text");

                    b.HasKey("IntegrationId")
                        .HasName("pk_integration_unique_constraints");

                    b.HasIndex("ProjectId", "Name")
                        .IsUnique();

                    b.HasIndex("TenantId", "IntegrationId")
                        .IsUnique();

                    b.ToTable("integration_unique_constraints");
                });
#pragma warning restore 612, 618
        }
    }
}
