using System.ComponentModel.DataAnnotations;

namespace ContaMente.DTOs;

public class UpdateUserProfileDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
