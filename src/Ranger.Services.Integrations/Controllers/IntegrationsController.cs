using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Geofences.Controllers
{
    [ApiController]
    public class IntegrationsController : ControllerBase
    {
        private readonly Func<string, IntegrationsRepository> integrationsRepositoryFactory;
        private readonly ITenantsClient tenantsClient;
        private readonly ILogger<IntegrationsController> logger;

        public IntegrationsController(Func<string, IntegrationsRepository> integrationsRepositoryFactory, ITenantsClient tenantsClient, ILogger<IntegrationsController> logger)
        {
            this.integrationsRepositoryFactory = integrationsRepositoryFactory;
            this.tenantsClient = tenantsClient;
            this.logger = logger;
        }

        [HttpGet("/{domain}/integrations")]
        public async Task<IActionResult> GetAllIntegrations([FromRoute] string domain, [FromQuery] Guid projectId)
        {
            var repo = integrationsRepositoryFactory(domain);

            try
            {
                var integrationVersionTuples = await repo.GetAllIntegrationsForProject(projectId);
                var integrationsList = new List<dynamic>();
                foreach (var result in integrationVersionTuples)
                {
                    dynamic integration = new ExpandoObject();
                    integration.Type = Enum.GetName(typeof(IntegrationsEnum), result.integrationType);
                    foreach (var propertyInfo in result.integration.GetType().GetProperties().Where(_ => _.Name.ToLowerInvariant() != "deleted"))
                    {
                        ((IDictionary<String, Object>)integration).Add(propertyInfo.Name, propertyInfo.GetValue(result.integration));
                    }
                    integration.Version = result.version;
                    integrationsList.Add(integration);
                }
                return Ok(integrationsList);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An exception occurred retrieving the requested integrations for domain '{domain}' and projectId '{projectId}'.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}