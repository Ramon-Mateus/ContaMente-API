using System.ComponentModel.DataAnnotations;

public class UpdateUserConfigurationDto
{
    [Required]
    public bool ListagemPorFatura { get; set; }
}
