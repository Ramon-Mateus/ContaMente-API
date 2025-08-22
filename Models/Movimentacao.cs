using System.Text.Json.Serialization;

namespace ContaMente.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }
        public double Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        public bool Fixa { get; set; }
        public int? NumeroParcela { get; set; }
        public TipoPagamentoEnum TipoPagamento { get; set; }
        [JsonIgnore]
        public int? CartaoId { get; set; }
        public Cartao? Cartao { get; set; }
        [JsonIgnore]
        public int CategoriaId { get; set; }
        [JsonIgnore]
        public int? ResponsavelId { get; set; }
        public Responsavel? Responsavel { get; set; }
        public Categoria? Categoria { get; set; }
        [JsonIgnore]
        public int? RecorrenciaId { get; set; }
        public Recorrencia? Recorrencia { get; set; }
        [JsonIgnore]
        public int? ParcelaId { get; set; }
        public Parcela? Parcela { get; set; }
    }
}
