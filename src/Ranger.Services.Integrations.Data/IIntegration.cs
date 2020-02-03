using System;
using Ranger.Common.SharedKernel;

namespace Ranger.Services.Integrations.Data
{
    public interface IIntegration
    {
        Guid Id { get; set; }
        EnvironmentEnum Environment { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        Guid ProjectId { get; set; }
        bool Enabled { get; set; }
        bool Deleted { get; set; }
    }
}