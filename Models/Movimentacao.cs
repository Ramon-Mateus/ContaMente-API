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
        [JsonIgnore]
        public int CategoriaId { get; set; }
        public int TipoPagamentoId { get; set; }
        public Categoria? Categoria { get; set; }
        public TipoPagamento? TipoPagamento { get; set; }
        public int? RecorrenciaId { get; set; }
        public Recorrencia? Recorrencia { get; set; }
        public int? ParcelaId { get; set; }
        public Parcela? Parcela { get; set; }
    }
}
