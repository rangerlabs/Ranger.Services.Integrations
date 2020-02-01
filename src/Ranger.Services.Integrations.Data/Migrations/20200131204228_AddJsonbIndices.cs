using Microsoft.EntityFrameworkCore.Migrations;

namespace Ranger.Services.Integrations.Data.Migrations
{
    public partial class AddJsonbIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //https://www.postgresql.org/docs/9.6/datatype-json.html
            //https://dba.stackexchange.com/questions/161313/creating-a-unique-constraint-from-a-json-object
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_data_integrationid_version ON integration_streams (database_username, (data->>'IntegrationId'), version);");
            migrationBuilder.Sql("CREATE INDEX idx_data_name ON integration_streams (database_username, (data->>'Name'));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX idx_data_integrationid_version");
            migrationBuilder.Sql("DROP INDEX idx_data_name");
        }
    }
}
