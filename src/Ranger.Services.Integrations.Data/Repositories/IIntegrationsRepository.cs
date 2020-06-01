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
        Task RemoveIntegrationStreamAsync(Guid projectId, string name);
        Task<Guid> SoftDeleteAsync(Guid projectId, string userEmail, string name);
        Task<IEnumerable<(IDomainIntegration integration, IntegrationsEnum integrationType, int version)>> GetAllIntegrationsForProject(Guid projectId);
        Task<IEnumerable<IDomainIntegration>> GetAllIntegrationsByIdForProject(Guid projectId, IEnumerable<Guid> integrationIds);
        Task<IEnumerable<IDomainIntegration>> GetAllIntegrationsForProjectIds(IEnumerable<Guid> projectIds);
        Task<Guid> GetIntegrationIdByCurrentNameAsync(Guid projectId, string name);
        Task<IDomainIntegration> GetIntegrationByIntegrationIdAsync(Guid projectId, Guid integrationId);
        Task UpdateIntegrationAsync(Guid projectId, string userEmail, string eventName, int version, IEntityIntegration integration);
    }
}