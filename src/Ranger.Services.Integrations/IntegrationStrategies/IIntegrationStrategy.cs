using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ranger.Common;
using Ranger.Services.Integrations.Data.DomainModels;

namespace Ranger.Services.Integrations
{
    public interface IIntegrationStrategy<TDomainIntegration>
    where TDomainIntegration : IDomainIntegration
    {
        Task Execute(string tenantId, string projectName, TDomainIntegration integration, IEnumerable<GeofenceIntegrationResult> geofenceIntegrationResult, Breadcrumb breadcrumb, EnvironmentEnum environment);
    }
}