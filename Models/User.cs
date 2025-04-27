using Microsoft.AspNetCore.Identity;

namespace ContaMente.Models;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}