using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Ranger.Services.Integrations.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(nullable: true),
                    xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_streams",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    database_username = table.Column<string>(nullable: false),
                    stream_id = table.Column<Guid>(nullable: false),
                    version = table.Column<int>(nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    @event = table.Column<string>(name: "event", nullable: false),
                    inserted_at = table.Column<DateTime>(nullable: false),
                    inserted_by = table.Column<string>(nullable: false),
                    integration_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_streams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_unique_constraints",
                columns: table => new
                {
                    integration_id = table.Column<Guid>(nullable: false),
                    database_username = table.Column<string>(nullable: false),
                    project_id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(maxLength: 140, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_unique_constraints", x => x.integration_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_integration_unique_constraints_database_username_integratio~",
                table: "integration_unique_constraints",
                columns: new[] { "database_username", "integration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_integration_unique_constraints_project_id_name",
                table: "integration_unique_constraints",
                columns: new[] { "project_id", "name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "integration_streams");

            migrationBuilder.DropTable(
                name: "integration_unique_constraints");
        }
    }
}
