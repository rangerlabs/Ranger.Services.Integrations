using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;
using Ranger.Services.Integrations.Data.EntityModels;

namespace Ranger.Services.Integrations.Data
{
    public interface IIntegrationsRepository : IRepository
    {
        Task AddIntegrationAsync(string userEmail, string eventName, IEntityIntegration integraiton, IntegrationsEnum integrationType);
        Task<Guid> SoftDeleteAsync(Guid projectId, string userEmail, string name);
        Task<IEnumerable<(IDomainIntegration integration, IntegrationsEnum integrationType, int version)>> GetAllIntegrationsForProject(Guid projectId);
        Task<IEnumerable<IDomainIntegration>> GetAllNotDeletedIntegrationsByIdsForProject(Guid projectId, IEnumerable<Guid> integrationIds);
        Task<IEnumerable<IDomainIntegration>> GetAllNotDeletedIntegrationsForProjectIds(IEnumerable<Guid> projectIds);
        Task<IDomainIntegration> GetNotDeletedIntegrationByIntegrationIdAsync(Guid projectId, Guid integrationId);
        Task UpdateIntegrationAsync(Guid projectId, string userEmail, string eventName, int version, IEntityIntegration integration);
    }
}