using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Services.Integrations.Data;

namespace Ranger.Services.Integrations
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class IntegrationsController : ControllerBase
    {
        private readonly Func<string, IntegrationsRepository> integrationsRepositoryFactory;
        private readonly ProjectsHttpClient projectsHttpClient;
        private readonly ILogger<IntegrationsController> logger;

        public IntegrationsController(Func<string, IntegrationsRepository> integrationsRepositoryFactory, ProjectsHttpClient projectsHttpClient, ILogger<IntegrationsController> logger)
        {
            this.integrationsRepositoryFactory = integrationsRepositoryFactory;
            this.projectsHttpClient = projectsHttpClient;
            this.logger = logger;
        }

        ///<summary>
        /// Gets all integrations for a tenant's project
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve integrations for</param>
        ///<param name="projectId">The project id to retrieve integrations for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/integrations/{tenantId}/{projectId}")]
        public async Task<ApiResponse> GetAllIntegrationsForProject(string tenantId, Guid projectId)
        {
            var repo = integrationsRepositoryFactory(tenantId);
            try
            {
                var integrationVersionTuples = await repo.GetAllIntegrationsForProject(projectId);
                var integrationsList = new List<dynamic>();
                foreach (var result in integrationVersionTuples)
                {
                    dynamic integration = new ExpandoObject();
                    integration.Type = getIntegrationTypeFriendlyName(result.integrationType);
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
                var message = "Failed to retrieve integrations";
                this.logger.LogError(ex, message);
                throw new ApiException(message, StatusCodes.Status500InternalServerError);
            }
        }

        ///<summary>
        /// Gets all integrations that are in use by an active project
        ///</summary>
        ///<param name="tenantId">The tenant id to retrieve integrations for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/integrations/{tenantId}/count")]
        public async Task<ApiResponse> GetAllActiveIntegrationsForTenant(string tenantId)
        {
            var projects = await projectsHttpClient.GetAllProjects<IEnumerable<Project>>(tenantId);
            var repo = integrationsRepositoryFactory(tenantId);
            try
            {
                var allIntegrations = await repo.GetAllIntegrationsForProjectIds(projects.Result.Select(p => p.ProjectId));
                return new ApiResponse("Successfully retrieved integrations", result: allIntegrations.Count());
            }
            catch (Exception ex)
            {
                var message = "Failed to retrieve integrations";
                this.logger.LogError(ex, message);
                throw new ApiException(message, StatusCodes.Status500InternalServerError);
            }
        }

        [NonAction]
        private string getIntegrationTypeFriendlyName(IntegrationsEnum integration)
        {
            return integration switch
            {
                IntegrationsEnum.WEBHOOK => "Webhook",
                _ => throw new ArgumentException($"Invalid integration type {integration}")
            };
        }
    }
}