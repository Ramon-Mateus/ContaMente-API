using System.Text.Json.Serialization;
using ContaMente.Models;

public enum StatusTransacaoPendente
{
    Pendente = 0,
    Aprovada = 1,
    Ignorada = 2
}

// Gasto importado do Pluggy aguardando aceite para virar uma Movimentacao.
// Não tem FK para a conta de propósito: se a conexão for removida, os registros
// aprovados/ignorados continuam aqui e a transação não volta a ser importada.
public class TransacaoPendente
{
    public int Id { get; set; }
    public string PluggyTransactionId { get; set; } = string.Empty;
    public string PluggyAccountId { get; set; } = string.Empty;
    public string? NomeConta { get; set; }
    public string? FinalCartao { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public double Valor { get; set; }
    public DateTime Data { get; set; }
    public string? CategoriaPluggy { get; set; }
    public int? NumeroParcela { get; set; }
    public int? TotalParcelas { get; set; }
    // Compras parceladas: todas as parcelas de uma compra viram um único registro identificado por esta chave.
    public string? ChaveCompra { get; set; }
    public double? ValorTotal { get; set; }
    public bool Pendente { get; set; } // status "PENDING" no Pluggy (ainda não lançada na fatura)
    public StatusTransacaoPendente Status { get; set; } = StatusTransacaoPendente.Pendente;
    public DateTime ImportadaEm { get; set; } = DateTime.UtcNow;
    public int? MovimentacaoId { get; set; }
    [JsonIgnore]
    public Movimentacao? Movimentacao { get; set; }
    public int? ParcelaId { get; set; }
    [JsonIgnore]
    public Parcela? Parcela { get; set; }
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    [JsonIgnore]
    public User? User { get; set; }
}
