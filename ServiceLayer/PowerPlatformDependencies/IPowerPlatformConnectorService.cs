namespace ServiceLayer.PowerPlatformDependencies
{
    public interface IPowerPlatformConnectorService
    {
        Task<bool> TestSnowflakeConnectionAsync();
        Task<List<dynamic>> ExecuteSnowflakeQueryAsync(string sqlQuery);
        Task<bool> SyncSnowflakeDataAsync(string datasetName);
    }

}
