using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
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
        private readonly ILogger<IntegrationsController> logger;

        public IntegrationsController(Func<string, IntegrationsRepository> integrationsRepositoryFactory, ILogger<IntegrationsController> logger)
        {
            this.integrationsRepositoryFactory = integrationsRepositoryFactory;
            this.logger = logger;
        }

        ///<summary>
        /// Gets all integrations for a tenant's project
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve integrations for</param>
        ///<param name="projectId">The project id to retrieve integrations for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{domain}/integrations/{projectId}")]
        public async Task<ApiResponse> GetAllIntegrations(string tenantId, Guid projectId)
        {
            var repo = integrationsRepositoryFactory(tenantId);
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
                return new ApiResponse("Successfully retrived integrations", integrationsList);
            }
            catch (Exception ex)
            {
                var message = "An error occurred retrieving geofences";
                this.logger.LogError(ex, message);
                throw new ApiException(message, StatusCodes.Status500InternalServerError);
            }
        }
    }
}