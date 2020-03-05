using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data
{
    public interface IIntegrationsRepository : IRepository
    {
        Task AddIntegrationAsync(string userEmail, string eventName, IIntegration integraiton, IntegrationsEnum integrationType);
        Task RemoveIntegrationStreamAsync(Guid projectId, string name);
        Task SoftDeleteAsync(Guid projectId, string userEmail, string name);
        Task<IEnumerable<(IIntegration integration, IntegrationsEnum integrationType, int version)>> GetAllIntegrationsForProject(Guid projectId);
        Task<Guid> GetIntegrationIdByCurrentNameAsync(Guid projectId, string name);
        Task<IIntegration> GetIntegrationByIntegrationIdAsync(Guid projectId, Guid integrationId);
        Task<IIntegration> UpdateIntegrationAsync(Guid projectId, string userEmail, string eventName, int version, IIntegration integration);
    }
}