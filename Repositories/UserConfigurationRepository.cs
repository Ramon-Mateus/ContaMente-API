

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
        var sql = @"
        INSERT INTO ""UserConfigurations"" (""UserId"", ""ListagemPorFatura"") 
        VALUES ({0}, {1}) 
        RETURNING ""Id""";

        var id = await _context.Database.ExecuteSqlRawAsync(sql,
            configuration.UserId,
            configuration.ListagemPorFatura);

        return new UserConfiguration
        {
            Id = id,
            UserId = configuration.UserId,
            ListagemPorFatura = configuration.ListagemPorFatura
        };
    }

    public async Task<UserConfiguration?> UpdateUserConfiguration(UserConfiguration configuration)
    {
        _context.UserConfigurations.Update(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }
}
