namespace ServiceLayer.PowerPlatformDependencies
{
    public interface ISnowflakeRepository
    {
        Task<T> ExecuteQueryAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null);
        Task<List<T>> ExecuteListQueryAsync<T>(string sqlQuery, Dictionary<string, object>? parameters = null);
        Task<int> ExecuteNonQueryAsync(string sqlStatement, Dictionary<string, object>? parameters = null);
        Task<bool> TestConnectionAsync();
    }

}
