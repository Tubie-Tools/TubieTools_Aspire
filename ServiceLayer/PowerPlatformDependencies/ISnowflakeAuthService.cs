namespace ServiceLayer.PowerPlatformDependencies
{
    public interface ISnowflakeAuthService
    {
        Task<string> GetAccessTokenAsync();
        Task<bool> ValidateTokenAsync();
    }

}
