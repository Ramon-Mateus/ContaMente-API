using System.ComponentModel.DataAnnotations;

public enum StatusDuplicataEnum
{
    [Display(Name = "Aberta")]
    Aberta = 1,

    [Display(Name = "Parcial")]
    Parcial = 2,

    [Display(Name = "Paga")]
    Paga = 3
}
