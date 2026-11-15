namespace ContaMente.DTOs.Pluggy
{
    public record RegistrarItemDto(string ItemId);

    public record ConnectTokenRequestDto(string? ItemId);

    public record VincularCartaoDto(int? CartaoId);

    public class TransacaoPendenteDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public double Valor { get; set; }
        public DateTime Data { get; set; }
        public string? NomeConta { get; set; }
        public string? FinalCartao { get; set; }
        public string? CategoriaPluggy { get; set; }
        public int? NumeroParcela { get; set; }
        public int? TotalParcelas { get; set; }
        public double? ValorTotal { get; set; }
        public bool Pendente { get; set; }
        public int Status { get; set; }
        public int? CartaoSugeridoId { get; set; }
        public int? MovimentacaoId { get; set; }
        public int? ParcelaId { get; set; }
    }
}
