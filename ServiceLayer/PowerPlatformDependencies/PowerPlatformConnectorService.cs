using Microsoft.Extensions.Logging;

namespace ServiceLayer.PowerPlatformDependencies
{
    public class PowerPlatformConnectorService : IPowerPlatformConnectorService
    {
        private readonly ISnowflakeRepository _snowflakeRepository;
        private readonly IPowerPlatformDataverseService _dataverseService;
        private readonly ILogger<PowerPlatformConnectorService> _logger;

        public PowerPlatformConnectorService(
            ISnowflakeRepository snowflakeRepository,
            IPowerPlatformDataverseService dataverseService,
            ILogger<PowerPlatformConnectorService> logger)
        {
            _snowflakeRepository = snowflakeRepository;
            _dataverseService = dataverseService;
            _logger = logger;
        }

        public async Task<bool> TestSnowflakeConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing Snowflake connection");
                return await _snowflakeRepository.TestConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Snowflake connection test failed");
                return false;
            }
        }

        public async Task<List<dynamic>> ExecuteSnowflakeQueryAsync(string sqlQuery)
        {
            try
            {
                _logger.LogInformation("Executing Snowflake query from Power Platform");
                return await _snowflakeRepository.ExecuteListQueryAsync<dynamic>(sqlQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Snowflake query");
                throw;
            }
        }

        public async Task<bool> SyncSnowflakeDataAsync(string datasetName)
        {
            try
            {
                _logger.LogInformation("Syncing Snowflake data for dataset: {DatasetName}", datasetName);

                // Implement sync logic based on dataset
                // Example: Query Snowflake, write to Dataverse
                var sqlQuery = $"SELECT * FROM {datasetName}";
                var data = await _snowflakeRepository.ExecuteListQueryAsync<dynamic>(sqlQuery);

                _logger.LogInformation("Sync completed for {DatasetName} with {RecordCount} records", datasetName, data.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing Snowflake data");
                throw;
            }
        }
    }

}
