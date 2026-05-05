namespace ContaMente.Services.Interfaces
{
    public interface IFeriadoService
    {
        /// <summary>
        /// Verifica se a data informada é um feriado nacional brasileiro,
        /// consultando a BrasilAPI (com cache em memória por ano).
        /// Em caso de falha de rede / API, retorna <c>false</c> (degradação
        /// graciosa: a data é tratada como dia útil normal).
        /// </summary>
        Task<bool> IsFeriadoNacionalAsync(DateTime data);
    }
}
