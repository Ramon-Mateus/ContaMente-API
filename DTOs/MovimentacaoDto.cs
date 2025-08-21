using ContaMente.Models;

public class MovimentacaoDto
{
        public int Id { get; set; }
        public double Valor { get; set; }
        public DateTime Data { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        public bool Fixa { get; set; }
        public int? NumeroParcela { get; set; }
        public TipoPagamentoDto? TipoPagamento { get; set; }
        public Responsavel? Responsavel { get; set; }
        public Categoria? Categoria { get; set; }
        public Recorrencia? Recorrencia { get; set; }
        public Parcela? Parcela { get; set; }
}