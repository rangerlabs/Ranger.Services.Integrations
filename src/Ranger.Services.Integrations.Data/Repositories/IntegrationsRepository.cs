using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Ranger.Common;
using Ranger.Common.Data.Exceptions;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public class IntegrationsRepository : IIntegrationsRepository
    {
        private readonly ContextTenant contextTenant;
        private readonly IntegrationsDbContext context;
        private readonly ILogger<IntegrationsRepository> logger;

        public IntegrationsRepository(ContextTenant contextTenant, IntegrationsDbContext context, ILogger<IntegrationsRepository> logger)
        {
            this.contextTenant = contextTenant;
            this.context = context;
            this.logger = logger;
        }

        public async Task AddIntegrationAsync(string userEmail, string eventName, IEntityIntegration integration, IntegrationsEnum integrationType)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"{nameof(eventName)} was null or whitespace");
            }
            if (integration is null)
            {
                throw new ArgumentNullException($"{nameof(integration)} was null");
            }

            var now = DateTime.UtcNow;
            integration.CreatedOn = now;
            var newIntegrationStream = new IntegrationStream()
            {
                TenantId = this.contextTenant.TenantId,
                StreamId = Guid.NewGuid(),
                Version = 0,
                Data = JsonConvert.SerializeObject(integration),
                IntegrationType = integrationType,
                Event = eventName,
                InsertedAt = now,
                InsertedBy = userEmail,
            };

            this.AddIntegrationUniqueConstraints(newIntegrationStream, integration);
            this.context.IntegrationStreams.Add(newIntegrationStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case IntegrationJsonbConstraintNames.Name:
                            {
                                throw new EventStreamDataConstraintException($"The integration name '{integration.Name}' is in use by another integration");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
        }

        public async Task<IEnumerable<IDomainIntegration>> GetAllNotDeletedIntegrationsForProjectIds(IEnumerable<Guid> projectIds)
        {
            if (projectIds is null)
            {
                throw new ArgumentNullException(nameof(projectIds));
            }

            logger.LogDebug("Querying all integrations for project ids {@ProjectIds}", projectIds);
            var integrationStreams = await this.context.IntegrationStreams
            .FromSqlRaw($@"
                    SELECT * FROM (
                        WITH active_integrations AS(
                            WITH max_versions AS (
                                    SELECT stream_id, MAX(version) AS version
                                    FROM integration_streams
                                    GROUP BY stream_id
                                )
                                SELECT i.stream_id, i.version
                                FROM max_versions mv, integration_streams i
                                WHERE i.stream_id = mv.stream_id
                                AND i.version = mv.version
                                AND (i.data ->> 'Deleted') = 'false'
                        )
                        SELECT 
                            i.id,
                            i.tenant_id,
                            i.stream_id,
                            i.version,
                            i.data,
                            i.integration_type,
                            i.event,
                            i.inserted_at,
                            i.inserted_by
                        FROM active_integrations ai, integration_streams i
                        WHERE i.stream_id = ai.stream_id
                        AND i.version = ai.version
                        AND (i.data ->> 'ProjectId') IN ('{String.Join("','", projectIds)}')) AS integrationstreams").ToListAsync();
            var integrationVersionTuples = new List<IDomainIntegration>();
            foreach (var integrationStream in integrationStreams)
            {
                var integration = JsonToEntityFactory.Factory(integrationStream.IntegrationType, integrationStream.Data);
                integrationVersionTuples.Add(
                    (
                        EntityToDomainFactory.Factory(integration)
                    )
                );
            }
            logger.LogDebug("Query returned {IntegrationCount} integrations", integrationStreams.Count);
            return integrationVersionTuples;
        }

        public async Task<IEnumerable<IDomainIntegration>> GetAllNotDeletedIntegrationsByIdsForProject(Guid projectId, IEnumerable<Guid> integrationIds)
        {
            var integrationStreams = await this.context.IntegrationStreams
            .FromSqlRaw($@"
                SELECT * FROM (
                    WITH not_deleted AS(
                        SELECT *
                        FROM integration_streams i, integration_unique_constraints iuc
                        WHERE iuc.project_id = '{projectId.ToString()}' AND (i.data ->> 'IntegrationId') = iuc.integration_id::text
                    )
                    SELECT DISTINCT ON (i.stream_id) 
                        i.id,
                        i.tenant_id,
                        i.stream_id,
                        i.version,
                        i.data,
                        i.integration_type,
                        i.event,
                        i.inserted_at,
                        i.inserted_by
                    FROM not_deleted i
                    WHERE (i.data ->> 'IntegrationId') IN ('{String.Join("','", integrationIds)}')
                    ORDER BY i.stream_id, i.version DESC) AS integrationstreams").ToListAsync();
            var integrationVersionTuples = new List<IDomainIntegration>();
            foreach (var integrationStream in integrationStreams)
            {
                var integration = JsonToEntityFactory.Factory(integrationStream.IntegrationType, integrationStream.Data);
                integrationVersionTuples.Add((EntityToDomainFactory.Factory(integration)));
            }
            return integrationVersionTuples;
        }

        public async Task<IEnumerable<(IDomainIntegration integration, IntegrationsEnum integrationType, int version)>> GetAllIntegrationsForProject(Guid projectId)
        {
            var integrationStreams = await this.context.IntegrationStreams.
            FromSqlInterpolated($@"
                SELECT * FROM (
                    WITH not_deleted AS(
                        SELECT  
                            i.id,
                            i.tenant_id,
                            i.stream_id,
                            i.version,
                            i.data,
                            i.integration_type,
                            i.event,
                            i.inserted_at,
                            i.inserted_by
                        FROM integration_streams i, integration_unique_constraints iuc
                        WHERE iuc.project_id = {projectId} AND (i.data ->> 'IntegrationId') = iuc.integration_id::text
                )
                SELECT DISTINCT ON (i.stream_id) 
                    i.id,
              		i.tenant_id,
              		i.stream_id,
              		i.version,
                    i.data,
                    i.integration_type,
            		i.event,
            		i.inserted_at,
            		i.inserted_by
                FROM not_deleted i
                ORDER BY i.stream_id, i.version DESC) AS integrationstreams").ToListAsync();
            var integrationVersionTuples = new List<(IDomainIntegration integration, IntegrationsEnum integrationType, int version)>();
            foreach (var integrationStream in integrationStreams)
            {
                var integration = JsonToEntityFactory.Factory(integrationStream.IntegrationType, integrationStream.Data);
                integrationVersionTuples.Add((EntityToDomainFactory.Factory(integration), integrationStream.IntegrationType, integrationStream.Version));
            }
            return integrationVersionTuples;
        }

        public async Task<IDomainIntegration> GetNotDeletedIntegrationByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            IntegrationStream integrationStream = await GetNotDeletedIntegrationStreamByIntegrationIdAsync(projectId, integrationId);

            var integration = JsonToEntityFactory.Factory(integrationStream.IntegrationType, integrationStream.Data);
            return EntityToDomainFactory.Factory(integration);
        }

        public async Task<Guid> SoftDeleteAsync(Guid projectId, string userEmail, string name)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace");
            }

            var currentIntegrationStream = await GetNotDeletedIntegrationStreamByIntegrationNameAsync(projectId, name);
            if (!(currentIntegrationStream is null))
            {
                var currentIntegration = JsonToEntityFactory.Factory(currentIntegrationStream.IntegrationType, currentIntegrationStream.Data);
                currentIntegration.Deleted = true;

                var deleted = false;
                var maxConcurrencyAttempts = 3;
                while (!deleted && maxConcurrencyAttempts != 0)
                {
                    var updatedIntegrationStream = new IntegrationStream()
                    {
                        TenantId = this.contextTenant.TenantId,
                        StreamId = currentIntegrationStream.StreamId,
                        Version = currentIntegrationStream.Version + 1,
                        Data = JsonConvert.SerializeObject(currentIntegration),
                        IntegrationType = currentIntegrationStream.IntegrationType,
                        Event = "IntegrationDeleted",
                        InsertedAt = DateTime.UtcNow,
                        InsertedBy = userEmail,
                    };
                    this.context.IntegrationUniqueConstraints.Remove(await this.context.IntegrationUniqueConstraints.Where(_ => _.IntegrationId == currentIntegration.IntegrationId).SingleAsync());
                    this.context.IntegrationStreams.Add(updatedIntegrationStream);
                    try
                    {
                        await this.context.SaveChangesAsync();
                        deleted = true;
                        logger.LogInformation($"Integration  {currentIntegration.Name} deleted");
                        return currentIntegration.IntegrationId;
                    }
                    catch (DbUpdateException ex)
                    {
                        var postgresException = ex.InnerException as PostgresException;
                        if (postgresException.SqlState == "23505")
                        {
                            var uniqueIndexViolation = postgresException.ConstraintName;
                            switch (uniqueIndexViolation)
                            {
                                case IntegrationJsonbConstraintNames.IntegrationId_Version:
                                    {
                                        logger.LogError($"The update version number was outdated. The current and updated stream versions are '{currentIntegrationStream.Version + 1}'");
                                        maxConcurrencyAttempts--;
                                        continue;
                                    }
                            }
                        }
                        throw;
                    }
                }
                if (!deleted)
                {
                    throw new ConcurrencyException($"After '{maxConcurrencyAttempts}' attempts, the version was still outdated. Too many updates have been applied in a short period of time. The current stream version is '{currentIntegrationStream.Version + 1}'. The integration was not deleted");
                }
            }
            throw new ArgumentException($"No integration was found with name '{name}' in project with id '{projectId}'");
        }

        public async Task UpdateIntegrationAsync(Guid projectId, string userEmail, string eventName, int version, IEntityIntegration integration)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"{nameof(eventName)} was null or whitespace");
            }
            if (integration is null)
            {
                throw new ArgumentException($"{nameof(integration)} was null");
            }

            var currentIntegrationStream = await this.GetNotDeletedIntegrationStreamByIntegrationIdAsync(projectId, integration.IntegrationId);
            if (currentIntegrationStream is null)
            {
                throw new Exception($"No integration was found for ProjectId '{integration.ProjectId}' and Integration Id '{integration.IntegrationId}'");
            }
            ValidateRequestVersionIncremented(version, currentIntegrationStream);

            var outdatedIntegration = JsonToEntityFactory.Factory(currentIntegrationStream.IntegrationType, currentIntegrationStream.Data);
            integration.ProjectId = outdatedIntegration.ProjectId;
            integration.Deleted = false;
            integration.CreatedOn = outdatedIntegration.CreatedOn;

            var serializedNewIntegrationData = JsonConvert.SerializeObject(integration);
            ValidateDataJsonInequality(currentIntegrationStream, serializedNewIntegrationData);

            var uniqueConstraint = await this.GetIntegrationUniqueConstraintsByIntegrationIdAsync(projectId, integration.IntegrationId);
            uniqueConstraint.Name = integration.Name.ToLowerInvariant();

            var updatedIntegrationStream = new IntegrationStream()
            {
                TenantId = this.contextTenant.TenantId,
                StreamId = currentIntegrationStream.StreamId,
                Version = version,
                Data = serializedNewIntegrationData,
                IntegrationType = currentIntegrationStream.IntegrationType,
                Event = eventName,
                InsertedAt = DateTime.UtcNow,
                InsertedBy = userEmail,
            };

            this.context.Update(uniqueConstraint);
            this.context.IntegrationStreams.Add(updatedIntegrationStream);
            try
            {
                await this.context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var postgresException = ex.InnerException as PostgresException;
                if (postgresException.SqlState == "23505")
                {
                    var uniqueIndexViolation = postgresException.ConstraintName;
                    switch (uniqueIndexViolation)
                    {
                        case IntegrationJsonbConstraintNames.Name:
                            {
                                throw new EventStreamDataConstraintException("The integration name is in use by another integration");
                            }
                        case IntegrationJsonbConstraintNames.IntegrationId_Version:
                            {
                                throw new ConcurrencyException($"The version number '{version}' was outdated. The current resource is at version '{currentIntegrationStream.Version}'. Re-request the resource to view the latest changes");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
        }

        private static void ValidateDataJsonInequality(IntegrationStream currentIntegrationStream, string serializedNewIntegrationData)
        {
            var currentJObject = JsonConvert.DeserializeObject<JObject>(currentIntegrationStream.Data);
            var requestJObject = JsonConvert.DeserializeObject<JObject>(serializedNewIntegrationData);
            if (JToken.DeepEquals(currentJObject, requestJObject))
            {
                throw new NoOpException("No changes were made from the previous version");
            }
        }

        private static void ValidateRequestVersionIncremented(int version, IntegrationStream currentIntegrationStream)
        {
            if (version - currentIntegrationStream.Version > 1)
            {
                throw new ConcurrencyException($"The version number '{version}' was too high. The current resource is at version '{currentIntegrationStream.Version}'");
            }
            if (version - currentIntegrationStream.Version <= 0)
            {
                throw new ConcurrencyException($"The version number '{version}' was outdated. The current resource is at version '{currentIntegrationStream.Version}'. Re-request the resource to view the latest changes");
            }
        }

        private async Task<IntegrationStream> GetNotDeletedIntegrationStreamByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            return await this.context.IntegrationStreams
            .FromSqlInterpolated($@"
                SELECT * FROM (
                    WITH not_deleted AS(
                        SELECT 
                            i.id,
                            i.tenant_id,
                            i.stream_id,
                            i.version,
                            i.data,
                            i.integration_type,
                            i.event,
                            i.inserted_at,
                            i.inserted_by
                        FROM integration_streams i, integration_unique_constraints iuc
                        WHERE iuc.project_id = '{projectId.ToString()}' 
                        AND iuc.integration_id = '{integrationId.ToString()}'
                        AND (i.data ->> 'IntegrationId') = iuc.integration_id::text
                )
                SELECT DISTINCT ON (i.stream_id) 
                    i.id,
                    i.tenant_id,
                    i.stream_id,
                    i.version,
                    i.data,
                    i.integration_type,
                    i.event,
                    i.inserted_at,
                    i.inserted_by
                FROM not_deleted i
                ORDER BY i.stream_id, i.version DESC) AS integrationstream").FirstOrDefaultAsync();
        }

        private async Task<IntegrationStream> GetNotDeletedIntegrationStreamByIntegrationNameAsync(Guid projectId, string name)
        {
            return await this.context.IntegrationStreams
            .FromSqlInterpolated($@"
                SELECT * FROM (
                    WITH not_deleted AS(
                        SELECT 
                            i.id,
                            i.tenant_id,
                            i.stream_id,
                            i.version,
                            i.data,
                            i.integration_type,
                            i.event,
                            i.inserted_at,
                            i.inserted_by
                        FROM integration_streams i, integration_unique_constraints iuc
                        WHERE iuc.project_id = '{projectId.ToString()}' 
                        AND iuc.name = '{name}'
                        AND (i.data ->> 'IntegrationId') = iuc.integration_id::text
                )
                SELECT DISTINCT ON (i.stream_id)      
                    i.id,
                    i.tenant_id,
                    i.stream_id,
                    i.version,
                    i.data,
                    i.integration_type,
                    i.event,
                    i.inserted_at,
                    i.inserted_by
                FROM not_deleted i
                ORDER BY i.stream_id, i.version DESC) AS integrationstream").FirstOrDefaultAsync();
        }

        public async Task<IntegrationUniqueConstraint> GetIntegrationUniqueConstraintsByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            return await this.context.IntegrationUniqueConstraints.SingleOrDefaultAsync(_ => _.ProjectId == projectId && _.IntegrationId == integrationId);
        }

        private void AddIntegrationUniqueConstraints(IntegrationStream IntegrationStream, IIntegration integration)
        {
            var newIntegrationUniqueConstraint = new IntegrationUniqueConstraint
            {
                IntegrationId = integration.IntegrationId,
                TenantId = contextTenant.TenantId,
                ProjectId = integration.ProjectId,
                Name = integration.Name.ToLowerInvariant(),
            };
            this.context.IntegrationUniqueConstraints.Add(newIntegrationUniqueConstraint);
        }
    }
}