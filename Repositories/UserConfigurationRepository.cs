

using ContaMente.Contexts;
using Microsoft.EntityFrameworkCore;

public class UserConfigurationRepository : IUserConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public UserConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserConfiguration?> GetUserConfiguration(string userId)
    {
        return await _context.UserConfigurations
            .FirstOrDefaultAsync(uc => uc.UserId == userId);
    }

    public async Task<UserConfiguration> CreateUserConfiguration(UserConfiguration configuration)
    {
        _context.UserConfigurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<UserConfiguration?> UpdateUserConfiguration(UserConfiguration configuration)
    {
        _context.UserConfigurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
}
