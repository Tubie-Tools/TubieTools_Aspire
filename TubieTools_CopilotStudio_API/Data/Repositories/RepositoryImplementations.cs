using Microsoft.EntityFrameworkCore;
using TubieTools_Aspire.EnterpriseAutomation.CopilotStudio.Models;

namespace TubieTools_CopilotStudio_API.Data.Repositories;

/// <summary>
/// Base repository with common CRUD implementation.
/// </summary>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly CopilotStudioDbContext _context;
    protected readonly ILogger<RepositoryBase<T>> _logger;

    protected RepositoryBase(CopilotStudioDbContext context, ILogger<RepositoryBase<T>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Entity added successfully");
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity");
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Entity updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity");
            throw;
        }
    }

    public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Entity with ID {Id} deleted successfully", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity with ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            throw;
        }
    }
}

/// <summary>
/// CopilotApplication repository implementation.
/// </summary>
public class CopilotApplicationRepository : RepositoryBase<CopilotApplication>, ICopilotApplicationRepository
{
    public CopilotApplicationRepository(CopilotStudioDbContext context, ILogger<CopilotApplicationRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<CopilotApplication>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default)
    {
        return await _context.CopilotApplications
            .Where(c => c.LandingZone == landingZone)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CopilotApplication>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CopilotApplications
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// KnowledgeTool repository implementation.
/// </summary>
public class KnowledgeToolRepository : RepositoryBase<KnowledgeTool>, IKnowledgeToolRepository
{
    public KnowledgeToolRepository(CopilotStudioDbContext context, ILogger<KnowledgeToolRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<KnowledgeTool>> GetAllWithoutFilterAsync(CancellationToken cancellationToken = default)
    {
        return await _context.KnowledgeTools.ToListAsync(cancellationToken);
    }
}

/// <summary>
/// GovernancePolicy repository implementation.
/// </summary>
public class GovernancePolicyRepository : RepositoryBase<CopilotGovernancePolicy>, IGovernancePolicyRepository
{
    public GovernancePolicyRepository(CopilotStudioDbContext context, ILogger<GovernancePolicyRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<CopilotGovernancePolicy>> GetByLandingZoneAsync(string landingZone, CancellationToken cancellationToken = default)
    {
        return await _context.GovernancePolicies
            .Where(p => p.LandingZone == landingZone)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// PerformanceMetrics repository implementation.
/// </summary>
public class PerformanceMetricsRepository : RepositoryBase<CopilotPerformanceMetrics>, IPerformanceMetricsRepository
{
    public PerformanceMetricsRepository(CopilotStudioDbContext context, ILogger<PerformanceMetricsRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<CopilotPerformanceMetrics>> GetRecentAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.PerformanceMetrics
            .Where(m => m.LastUpdated >= cutoffDate)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// DeploymentConfig repository implementation.
/// </summary>
public class DeploymentConfigRepository : RepositoryBase<CopilotDeploymentConfig>, IDeploymentConfigRepository
{
    public DeploymentConfigRepository(CopilotStudioDbContext context, ILogger<DeploymentConfigRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<CopilotDeploymentConfig>> GetByEnvironmentAsync(string environment, CancellationToken cancellationToken = default)
    {
        return await _context.DeploymentConfigs
            .Where(c => c.Environment == environment)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Version repository implementation.
/// </summary>
public class VersionRepository : RepositoryBase<CopilotVersion>, IVersionRepository
{
    public VersionRepository(CopilotStudioDbContext context, ILogger<VersionRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<CopilotVersion?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Versions
                .OrderByDescending(v => v.ReleaseDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest version");
            throw;
        }
    }

    public async Task<IEnumerable<CopilotVersion>> GetByVersionNumberAsync(
        string versionNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Versions
                .Where(v => v.VersionNumber == versionNumber)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting versions by number {VersionNumber}", versionNumber);
            throw;
        }
    }
    
}
