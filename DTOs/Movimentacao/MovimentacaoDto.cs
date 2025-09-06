using ContaMente.DTOs;
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
        public ResponsavelDto? Responsavel { get; set; }
        public CategoriaDto? Categoria { get; set; }
        public RecorrenciaDto? Recorrencia { get; set; }
        public ParcelaDto? Parcela { get; set; }
        public CartaoDto? Cartao { get; set; }
}