    public class ParcelaDto
    {
        public int Id { get; set; }
        public double ValorTotal { get; set; }
        public int NumeroParcelas { get; set; }
        public double ValorParcela { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; } = null;
    }