using System.Collections.Concurrent;
using System.Text.Json;
using ContaMente.Services.Interfaces;

namespace ContaMente.Services
{
    /// <summary>
    /// Serviço de feriados nacionais brasileiros baseado na BrasilAPI.
    ///
    /// Endpoint: <c>GET https://brasilapi.com.br/api/feriados/v1/{ano}</c>
    ///
    /// Estratégia de cache:
    ///   - Singleton no DI (uma instância pra toda a aplicação).
    ///   - Cache em memória por ano (<see cref="ConcurrentDictionary{TKey,TValue}"/>).
    ///   - Carrega o ano sob demanda na primeira consulta e mantém para
    ///     sempre — só é descartado quando o app é reiniciado.
    ///   - Um <see cref="SemaphoreSlim"/> por ano evita que requisições
    ///     simultâneas batam na API ao mesmo tempo na primeira carga.
    ///
    /// Estratégia de fallback:
    ///   - Se a chamada HTTP falhar (timeout, erro de rede, 5xx etc.),
    ///     o método <see cref="IsFeriadoNacionalAsync"/> apenas registra um
    ///     warning e retorna <c>false</c>. O lançamento da conta segue normal,
    ///     considerando apenas finais de semana — sem "estourar" pra cima do
    ///     usuário (Opção B definida com o cliente).
    /// </summary>
    public class FeriadoService : IFeriadoService
    {
        private const string HttpClientName = "BrasilApi";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FeriadoService> _logger;

        private readonly ConcurrentDictionary<int, HashSet<DateOnly>> _cache = new();
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

        public FeriadoService(IHttpClientFactory httpClientFactory, ILogger<FeriadoService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> IsFeriadoNacionalAsync(DateTime data)
        {
            try
            {
                var feriados = await GetFeriadosAnoAsync(data.Year);
                return feriados.Contains(DateOnly.FromDateTime(data));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha ao consultar feriados na BrasilAPI para o ano {Ano}. A data {Data:yyyy-MM-dd} será considerada dia útil.",
                    data.Year,
                    data
                );
                return false;
            }
        }

        private async Task<HashSet<DateOnly>> GetFeriadosAnoAsync(int ano)
        {
            if (_cache.TryGetValue(ano, out var existing))
                return existing;

            var gate = _locks.GetOrAdd(ano, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                if (_cache.TryGetValue(ano, out existing))
                    return existing;

                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync($"api/feriados/v1/{ano}");
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                var feriados = await JsonSerializer.DeserializeAsync<List<FeriadoBrasilApiDto>>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<FeriadoBrasilApiDto>();

                var datas = feriados
                    .Where(f => DateOnly.TryParse(f.Date, out _))
                    .Select(f => DateOnly.Parse(f.Date))
                    .ToHashSet();

                _cache[ano] = datas;
                _logger.LogInformation(
                    "Feriados de {Ano} carregados da BrasilAPI: {Total} datas em cache.",
                    ano,
                    datas.Count
                );
                return datas;
            }
            finally
            {
                gate.Release();
            }
        }

        private sealed class FeriadoBrasilApiDto
        {
            public string Date { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }
    }
}
