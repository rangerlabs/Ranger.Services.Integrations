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
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{

    //TODO: use the new System.Text.Json API to query
    public class IntegrationsRepository : IIntegrationsRepository
    {
        private readonly ContextTenant contextTenant;
        private readonly IntegrationsDbContext context;
        private readonly CloudSqlOptions cloudSqlOptions;
        private readonly ILogger<IntegrationsRepository> logger;

        public IntegrationsRepository(ContextTenant contextTenant, IntegrationsDbContext context, ILogger<IntegrationsRepository> logger)
        {
            this.contextTenant = contextTenant;
            this.context = context;
            this.logger = logger;
        }

        public async Task AddIntegrationAsync(string userEmail, string eventName, IIntegration integration, IntegrationsEnum integrationType)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"{nameof(eventName)} was null or whitespace.");
            }
            if (integration is null)
            {
                throw new ArgumentNullException($"{nameof(integration)} was null.");
            }

            var newIntegrationStream = new IntegrationStream()
            {
                DatabaseUsername = this.contextTenant.DatabaseUsername,
                StreamId = Guid.NewGuid(),
                Version = 0,
                Data = JsonConvert.SerializeObject(integration),
                IntegrationType = integrationType,
                Event = eventName,
                InsertedAt = DateTime.UtcNow,
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
                                throw new EventStreamDataConstraintException("The integration name is in use by another integration.");
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

        public async Task<IEnumerable<(IIntegration integration, int version)>> GetAllIntegrationsForProject(Guid projectId)
        {
            var IntegrationStreams = await this.context.IntegrationStreams.
            FromSqlInterpolated($@"
                WITH not_deleted AS(
	                SELECT 
            	    	is.id,
            	    	is.database_username,
            	    	is.stream_id,
            	    	is.version,
            	    	is.data,
                        is.integration_type,
            	    	is.event,
            	    	is.inserted_at,
            	    	is.inserted_by
            	    FROM integration_streams is, integration_unique_constraints iuc
            	    WHERE iuc.ProjectId = {projectId.ToString()} AND (is.data ->> 'IntegrationId') = iuc.integration_id::text
               )
               SELECT DISTINCT ON (is.stream_id) 
              		is.id,
              		is.database_username,
              		is.stream_id,
              		is.version,
                    is.data,
                    is.integration_type,
            		is.event,
            		is.inserted_at,
            		is.inserted_by
                FROM not_deleted is
                ORDER BY is.stream_id, is.version DESC;").ToListAsync();
            List<(IIntegration integration, int version)> integrationVersionTuples = new List<(IIntegration integration, int version)>();
            foreach (var integrationStream in IntegrationStreams)
            {
                var (integration, _) = IntegrationMessageTypeFactory.Factory(integrationStream.IntegrationType, null);
                integrationVersionTuples.Add((integration, integrationStream.Version));
            }
            return integrationVersionTuples;
        }

        public async Task<IIntegration> GetIntegrationByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            var integrationStream = await this.context.IntegrationStreams.FromSqlInterpolated($"SELECT * FROM integration_streams WHERE data ->> 'ProjectId' = {projectId.ToString()} AND data ->> 'IntegrationId' = {integrationId.ToString()} AND data ->> 'Deleted' = 'false' ORDER BY version DESC").FirstOrDefaultAsync();
            var (_, type) = IntegrationMessageTypeFactory.Factory(integrationStream.IntegrationType, null);
            return JsonConvert.DeserializeObject(integrationStream.Data, type) as IIntegration;
        }

        public async Task<Guid> GetIntegrationIdByCurrentNameAsync(Guid projectId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} was null or whitespace.");
            }

            return await this.context.IntegrationUniqueConstraints.Where(_ => _.ProjectId == projectId && _.Name == name).Select(_ => _.IntegrationId).SingleOrDefaultAsync();
        }

        public async Task RemoveIntegrationStreamAsync(Guid projectId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} was null or whitespace.");
            }
            this.context.IntegrationStreams.RemoveRange(
                await this.context.IntegrationStreams.FromSqlInterpolated($"SELECT * FROM integration_streams WHERE data ->> 'ProjectId' = {projectId.ToString()} AND data ->> 'Name' = {name} ORDER BY version DESC").ToListAsync()
            );
        }

        public async Task SoftDeleteAsync(Guid projectId, string userEmail, Guid integrationId)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }

            var currentIntegrationStream = await GetIntegrationStreamByIntegrationIdAsync(projectId, integrationId);
            if (!(currentIntegrationStream is null))
            {
                var (currentIntegration, _) = IntegrationMessageTypeFactory.Factory(currentIntegrationStream.IntegrationType, currentIntegrationStream.Data);
                currentIntegration.Deleted = true;

                var deleted = false;
                var maxConcurrencyAttempts = 3;
                while (!deleted && maxConcurrencyAttempts != 0)
                {
                    var updatedIntegrationStream = new IntegrationStream()
                    {
                        DatabaseUsername = this.contextTenant.DatabaseUsername,
                        StreamId = currentIntegrationStream.StreamId,
                        Version = currentIntegrationStream.Version + 1,
                        Data = JsonConvert.SerializeObject(currentIntegration),
                        IntegrationType = currentIntegrationStream.IntegrationType,
                        Event = "IntegrationDeleted",
                        InsertedAt = DateTime.UtcNow,
                        InsertedBy = userEmail,
                    };
                    this.context.IntegrationUniqueConstraints.Remove(await this.context.IntegrationUniqueConstraints.Where(_ => _.IntegrationId == currentIntegration.Id).SingleAsync());
                    this.context.IntegrationStreams.Add(updatedIntegrationStream);
                    try
                    {
                        await this.context.SaveChangesAsync();
                        deleted = true;
                        logger.LogInformation($"Integration {currentIntegration.Name} deleted.");
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
                                        logger.LogError($"The update version number was outdated. The current and updated stream versions are '{currentIntegrationStream.Version + 1}'.");
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
                    throw new ConcurrencyException($"After '{maxConcurrencyAttempts}' attempts, the version was still outdated. Too many updates have been applied in a short period of time. The current stream version is '{currentIntegrationStream.Version + 1}'. The integration was not deleted.");
                }
            }
            else
            {
                throw new ArgumentException($"No integration was found with id '{integrationId}' in project with id '{projectId}'.");
            }
        }

        public async Task<IIntegration> UpdateIntegrationAsync(Guid projectId, string userEmail, string eventName, int version, IIntegration integration)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException($"{nameof(userEmail)} was null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"{nameof(eventName)} was null or whitespace.");
            }
            if (integration is null)
            {
                throw new ArgumentException($"{nameof(integration)} was null.");
            }

            var currentIntegrationStream = await GetIntegrationStreamByIntegrationIdAsync(projectId, integration.Id);
            ValidateRequestVersionIncremented(version, currentIntegrationStream);

            var (outdatedIntegration, _) = IntegrationMessageTypeFactory.Factory(currentIntegrationStream.IntegrationType, currentIntegrationStream.Data);
            integration.Deleted = false;

            var serializedNewIntegrationData = JsonConvert.SerializeObject(integration);
            ValidateDataJsonInequality(currentIntegrationStream, serializedNewIntegrationData);

            var uniqueConstraint = await this.GetIntegrationUniqueConstraintsByIntegrationIdAsync(projectId, integration.Id);
            uniqueConstraint.Name = integration.Name;

            var updatedIntegrationStream = new IntegrationStream()
            {
                DatabaseUsername = this.contextTenant.DatabaseUsername,
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
                                throw new EventStreamDataConstraintException("The integration name is in use by another integration.");
                            }
                        case IntegrationJsonbConstraintNames.IntegrationId_Version:
                            {
                                throw new ConcurrencyException($"The update version number was outdated. The current stream version is '{currentIntegrationStream.Version}' and the request update version was '{version}'.");
                            }
                        default:
                            {
                                throw new EventStreamDataConstraintException("");
                            }
                    }
                }
                throw;
            }
            return integration;
        }

        private async Task<IntegrationStream> GetIntegrationStreamByIntegrationNameAsync(Guid projectId, string name)
        {
            return await this.context.IntegrationStreams.FromSqlInterpolated($"SELECT * FROM integration_streams WHERE data ->> 'ProjectId' = {projectId.ToString()} AND data ->> 'Name' = {name} AND data ->> 'Deleted' = 'false' ORDER BY version DESC").FirstOrDefaultAsync();
        }

        private static void ValidateDataJsonInequality(IntegrationStream currentIntegrationStream, string serializedNewIntegrationData)
        {
            var currentJObject = JsonConvert.DeserializeObject<JObject>(currentIntegrationStream.Data);
            var requestJObject = JsonConvert.DeserializeObject<JObject>(serializedNewIntegrationData);
            if (JToken.DeepEquals(currentJObject, requestJObject))
            {
                throw new NoOpException("No changes were made from the previous version.");
            }
        }

        private static void ValidateRequestVersionIncremented(int version, IntegrationStream currentIntegrationStream)
        {
            if (version - currentIntegrationStream.Version > 1)
            {
                throw new ConcurrencyException($"The update version number was too high. The current stream version is '{currentIntegrationStream.Version}' and the request update version was '{version}'.");
            }
            if (version - currentIntegrationStream.Version <= 0)
            {
                throw new ConcurrencyException($"The update version number was outdated. The current stream version is '{currentIntegrationStream.Version}' and the request update version was '{version}'.");
            }
        }

        private async Task<IntegrationStream> GetIntegrationStreamByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            return await this.context.IntegrationStreams.FromSqlInterpolated($"SELECT * FROM integration_streams WHERE data ->> 'ProjectId' = {projectId.ToString()} AND data ->> 'IntegrationId' = {integrationId.ToString()} AND data -> 'Deleted' = 'false' ORDER BY version DESC").FirstOrDefaultAsync();
        }

        public async Task<IntegrationUniqueConstraint> GetIntegrationUniqueConstraintsByIntegrationIdAsync(Guid projectId, Guid integrationId)
        {
            return await this.context.IntegrationUniqueConstraints.SingleOrDefaultAsync(_ => _.ProjectId == projectId && _.IntegrationId == integrationId);
        }

        private void AddIntegrationUniqueConstraints(IntegrationStream IntegrationStream, IIntegration integration)
        {
            var newIntegrationUniqueConstraint = new IntegrationUniqueConstraint
            {
                IntegrationId = integration.Id,
                DatabaseUsername = contextTenant.DatabaseUsername,
                ProjectId = integration.ProjectId,
                Name = integration.Name,
            };
            this.context.IntegrationUniqueConstraints.Add(newIntegrationUniqueConstraint);
        }
    }
}