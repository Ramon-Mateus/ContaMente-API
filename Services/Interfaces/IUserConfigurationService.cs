public interface IUserConfigurationService
{
    Task<UserConfiguration?> GetUserConfiguration(string userId);
    Task<UserConfiguration> CreateUserConfiguration(string userId);
    Task<UserConfiguration?> UpdateUserConfiguration(UpdateUserConfigurationDto updateUserConfigurationDto, string userId);
}
