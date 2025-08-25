public class UserConfigurationService : IUserConfigurationService
{
    private readonly IUserConfigurationRepository _userConfigurationRepository;

    public UserConfigurationService(IUserConfigurationRepository userConfigurationRepository)
    {
        _userConfigurationRepository = userConfigurationRepository;
    }

    public async Task<UserConfiguration?> GetUserConfiguration(string userId)
    {
        return await _userConfigurationRepository.GetUserConfiguration(userId);
    }

    public async Task<UserConfiguration> CreateUserConfiguration(string userId)
    {
        var configuration = new UserConfiguration
        {
            ListagemPorFatura = false,
            UserId = userId
        };

        return await _userConfigurationRepository.CreateUserConfiguration(configuration);
    }
    public async Task<UserConfiguration?> UpdateUserConfiguration(UpdateUserConfigurationDto updateUserConfigurationDto, string userId)
    {
        var configuration = await this.GetUserConfiguration(userId);

        if (configuration == null)
        {
            return null;
        }
        
        configuration.ListagemPorFatura = updateUserConfigurationDto.ListagemPorFatura;

        return await _userConfigurationRepository.UpdateUserConfiguration(configuration);
    }
}