using System;
using System.Threading;
using System.Threading.Tasks;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data
{
    public interface IIntegrationUniqueContraintRepository
    {
        Task<bool> GetIntegrationNameAvailableByProjectAsync(Guid projectId, string name, CancellationToken cancellationToken);
    }
}