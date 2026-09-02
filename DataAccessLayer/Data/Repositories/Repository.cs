using DataAccessLayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Data.Repositories
{
    /// <summary>
    /// Generic repository interface for common CRUD operations.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for CopilotApplication entities.
    /// </summary>
    public interface ICopilotApplicationRepository : IRepository<CopilotApplication>
    {
        Task<IEnumerable<CopilotApplication>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default);
        Task<IEnumerable<CopilotApplication>> GetActiveAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for KnowledgeTool entities.
    /// </summary>
    public interface IKnowledgeToolRepository : IRepository<KnowledgeTool>
    {
        Task<IEnumerable<KnowledgeTool>> GetAllWithoutFilterAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for CopilotGovernancePolicy entities.
    /// </summary>
    public interface IGovernancePolicyRepository : IRepository<CopilotGovernancePolicy>
    {
        Task<IEnumerable<CopilotGovernancePolicy>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for CopilotPerformanceMetrics entities.
    /// </summary>
    public interface IPerformanceMetricsRepository : IRepository<CopilotPerformanceMetrics>
    {
        Task<IEnumerable<CopilotPerformanceMetrics>> GetRecentAsync(int days = 30, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for CopilotDeploymentConfig entities.
    /// </summary>
    public interface IDeploymentConfigRepository : IRepository<CopilotDeploymentConfig>
    {
        Task<IEnumerable<CopilotDeploymentConfig>> GetByEnvironmentAsync(string environment, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository for CopilotVersion entities.
    /// </summary>
    public interface IVersionRepository : IRepository<CopilotVersion>
    {
        Task<CopilotVersion?> GetLatestAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CopilotVersion>> GetByVersionNumberAsync(string versionNumber, CancellationToken cancellationToken = default);
    }
}
