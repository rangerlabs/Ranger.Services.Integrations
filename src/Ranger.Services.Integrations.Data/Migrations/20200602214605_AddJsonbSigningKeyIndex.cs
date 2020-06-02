using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Integrations.Data.Migrations
{
    public partial class AddJsonbSigningKeyIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_data_signing_key ON integration_streams (tenant_id, (data->>'SigningKey'));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_data_signing_key");
        }
    }
}
