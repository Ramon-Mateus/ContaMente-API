public interface IUserConfigurationRepository
{
    Task<UserConfiguration?> GetUserConfiguration(string userId);
    Task<UserConfiguration> CreateUserConfiguration(UserConfiguration configuration);
    Task<UserConfiguration?> UpdateUserConfiguration(UserConfiguration configuration);
}