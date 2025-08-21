using System.ComponentModel.DataAnnotations;

public enum TipoPagamentoEnum
{
    [Display(Name = "Crédito")]
    Credito = 1,

    [Display(Name = "Pix")]
    Pix = 2,

    [Display(Name = "Dinheiro")]
    Dinheiro = 3,

    [Display(Name = "Boleto")]
    Boleto = 4,

    [Display(Name = "Débito")]
    Debito = 5
}