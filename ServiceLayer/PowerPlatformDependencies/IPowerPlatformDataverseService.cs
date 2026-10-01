namespace ServiceLayer.PowerPlatformDependencies
{
    public interface IPowerPlatformDataverseService
    {
        Task<T> GetRecordAsync<T>(string tableName, string recordId);
        Task<List<T>> GetRecordsAsync<T>(string tableName, string fetchXml);
        Task<string> CreateRecordAsync(string tableName, Dictionary<string, object> attributes);
        Task<bool> UpdateRecordAsync(string tableName, string recordId, Dictionary<string, object> attributes);
        Task<bool> DeleteRecordAsync(string tableName, string recordId);
    }

}
