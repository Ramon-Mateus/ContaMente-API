using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContaMente.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContaMente.Services.Pluggy
{
    #region Modelos da API do Pluggy
    public class PluggyPage<T>
    {
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public List<T> Results { get; set; } = new();
    }

    public class PluggyCursorPage<T>
    {
        public List<T> Results { get; set; } = new();
        public string? Next { get; set; }
    }

    public class PluggyApiItem
    {
        public string Id { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? ExecutionStatus { get; set; }
        public string? ClientUserId { get; set; }
        public PluggyApiConnector? Connector { get; set; }
        public PluggyApiError? Error { get; set; }
    }

    public class PluggyApiConnector
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class PluggyApiError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }

    public class PluggyApiAccount
    {
        public string Id { get; set; } = string.Empty;
        public string? Type { get; set; }     // BANK | CREDIT
        public string? Subtype { get; set; }  // CREDIT_CARD | CHECKING_ACCOUNT ...
        public string? Name { get; set; }
        public string? MarketingName { get; set; }
        public string? Number { get; set; }
    }

    public class PluggyApiTransaction
    {
        public string Id { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DescriptionRaw { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; } // POSTED | PENDING
        public string? Type { get; set; }   // DEBIT (saída) | CREDIT (entrada/estorno/pagamento)
        public string? Category { get; set; }
        public PluggyApiCreditCardMetadata? CreditCardMetadata { get; set; }
        public PluggyApiMerchant? Merchant { get; set; }
    }

    public class PluggyApiCreditCardMetadata
    {
        public int? InstallmentNumber { get; set; }
        public int? TotalInstallments { get; set; }
        public string? CardNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public double? TotalAmount { get; set; }
    }

    public class PluggyApiMerchant
    {
        public string? Name { get; set; }
        public string? BusinessName { get; set; }
    }
    #endregion

    public interface IPluggyClient
    {
        Task ValidarCredenciais();
        Task<string> CreateConnectToken(string clientUserId, string? itemId = null);
        Task<PluggyApiItem> GetItem(string itemId);
        Task UpdateItem(string itemId);
        Task DeleteItem(string itemId);
        Task<List<PluggyApiAccount>> GetAccounts(string itemId);
        Task<List<PluggyApiTransaction>> GetTransactions(string accountId, DateTime from);
    }

    public interface IPluggyClientFactory
    {
        IPluggyClient Criar(string clientId, string clientSecret);
        // Cliente com as credenciais que o usuário cadastrou no perfil; null se ele não cadastrou.
        Task<IPluggyClient?> ParaUsuario(string userId);
    }

    public class PluggyClientFactory : IPluggyClientFactory
    {
        public const string NomeHttpClient = "pluggy";

        private readonly IHttpClientFactory _httpFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PluggyClient> _logger;

        public PluggyClientFactory(IHttpClientFactory httpFactory, ApplicationDbContext context, ILogger<PluggyClient> logger)
        {
            _httpFactory = httpFactory;
            _context = context;
            _logger = logger;
        }

        public IPluggyClient Criar(string clientId, string clientSecret) =>
            new PluggyClient(_httpFactory.CreateClient(NomeHttpClient), _logger, clientId, clientSecret);

        public async Task<IPluggyClient?> ParaUsuario(string userId)
        {
            var cred = await _context.PluggyCredenciais.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
            if (cred == null) return null;
            return Criar(cred.ClientId, CredencialCripto.Descriptografar(cred.ClientSecretCriptografado));
        }
    }

    public class PluggyClient : IPluggyClient
    {
        // API keys em cache por conta Pluggy (cada usuário tem a sua).
        private static readonly ConcurrentDictionary<string, (string ApiKey, DateTime ExpiraEm)> _apiKeys = new();
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly HttpClient _http;
        private readonly ILogger<PluggyClient> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _chaveCache;

        public PluggyClient(HttpClient http, ILogger<PluggyClient> logger, string clientId, string clientSecret)
        {
            _http = http;
            _http.BaseAddress ??= new Uri(Environment.GetEnvironmentVariable("PLUGGY_API_URL") ?? "https://api.pluggy.ai/");
            _logger = logger;
            _clientId = clientId;
            _clientSecret = clientSecret;
            // O secret entra no hash para que trocá-lo invalide a key em cache.
            _chaveCache = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
        }

        public async Task ValidarCredenciais() => await GetApiKey(forcarRenovacao: true);

        // A API key do Pluggy vale 2h; renovamos com folga.
        private async Task<string> GetApiKey(bool forcarRenovacao = false)
        {
            if (!forcarRenovacao && _apiKeys.TryGetValue(_chaveCache, out var c) && DateTime.UtcNow < c.ExpiraEm)
                return c.ApiKey;

            var trava = _locks.GetOrAdd(_chaveCache, _ => new SemaphoreSlim(1, 1));
            await trava.WaitAsync();
            try
            {
                if (!forcarRenovacao && _apiKeys.TryGetValue(_chaveCache, out c) && DateTime.UtcNow < c.ExpiraEm)
                    return c.ApiKey;

                var resp = await _http.PostAsJsonAsync("auth", new { clientId = _clientId, clientSecret = _clientSecret }, _json);
                if (resp.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden
                    or System.Net.HttpStatusCode.BadRequest)
                    throw new ArgumentException("Credenciais do Pluggy inválidas. Confira o Client ID e o Client Secret no seu perfil.");
                await GarantirSucesso(resp, "autenticar");
                var body = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
                var apiKey = body.GetProperty("apiKey").GetString()!;
                _apiKeys[_chaveCache] = (apiKey, DateTime.UtcNow.AddMinutes(100));
                return apiKey;
            }
            finally
            {
                trava.Release();
            }
        }

        private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null)
        {
            async Task<HttpResponseMessage> Tentar(bool renovar)
            {
                var req = new HttpRequestMessage(method, path);
                req.Headers.Add("X-API-KEY", await GetApiKey(renovar));
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (body != null)
                    req.Content = JsonContent.Create(body, options: _json);
                return await _http.SendAsync(req);
            }

            var resp = await Tentar(false);
            if (resp.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                resp.Dispose();
                resp = await Tentar(true);
            }
            return resp;
        }

        private async Task GarantirSucesso(HttpResponseMessage resp, string operacao)
        {
            if (resp.IsSuccessStatusCode) return;
            var conteudo = await resp.Content.ReadAsStringAsync();
            _logger.LogError("Pluggy: falha ao {Operacao} ({Status}): {Conteudo}", operacao, (int)resp.StatusCode, conteudo);
            throw new HttpRequestException($"Pluggy: falha ao {operacao} ({(int)resp.StatusCode}).");
        }

        public async Task<string> CreateConnectToken(string clientUserId, string? itemId = null)
        {
            var webhookUrl = Environment.GetEnvironmentVariable("PLUGGY_WEBHOOK_URL");
            var webhookSecret = Environment.GetEnvironmentVariable("PLUGGY_WEBHOOK_SECRET");
            if (!string.IsNullOrWhiteSpace(webhookUrl) && !string.IsNullOrWhiteSpace(webhookSecret))
                webhookUrl += (webhookUrl.Contains('?') ? "&" : "?") + "secret=" + Uri.EscapeDataString(webhookSecret);

            var payload = new
            {
                itemId,
                options = new
                {
                    clientUserId,
                    webhookUrl = string.IsNullOrWhiteSpace(webhookUrl) ? null : webhookUrl
                }
            };

            using var resp = await Send(HttpMethod.Post, "connect_token", payload);
            await GarantirSucesso(resp, "criar connect token");
            var body = await resp.Content.ReadFromJsonAsync<JsonElement>(_json);
            return body.GetProperty("accessToken").GetString()!;
        }

        public async Task<PluggyApiItem> GetItem(string itemId)
        {
            using var resp = await Send(HttpMethod.Get, $"items/{Uri.EscapeDataString(itemId)}");
            if (resp.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.BadRequest)
                throw new KeyNotFoundException("Conexão não encontrada no Pluggy.");
            await GarantirSucesso(resp, "buscar item");
            return (await resp.Content.ReadFromJsonAsync<PluggyApiItem>(_json))!;
        }

        public async Task UpdateItem(string itemId)
        {
            using var resp = await Send(HttpMethod.Patch, $"items/{Uri.EscapeDataString(itemId)}", new { });
            await GarantirSucesso(resp, "atualizar item");
        }

        public async Task DeleteItem(string itemId)
        {
            using var resp = await Send(HttpMethod.Delete, $"items/{Uri.EscapeDataString(itemId)}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return;
            await GarantirSucesso(resp, "remover item");
        }

        public async Task<List<PluggyApiAccount>> GetAccounts(string itemId)
        {
            using var resp = await Send(HttpMethod.Get, $"accounts?itemId={Uri.EscapeDataString(itemId)}");
            await GarantirSucesso(resp, "listar contas");
            var page = await resp.Content.ReadFromJsonAsync<PluggyPage<PluggyApiAccount>>(_json);
            return page?.Results ?? new();
        }

        // GET /v2/transactions: filtro por dateFrom e paginação por cursor ("next" = query string da próxima página).
        public async Task<List<PluggyApiTransaction>> GetTransactions(string accountId, DateTime from)
        {
            var todas = new List<PluggyApiTransaction>();
            string? query = $"?accountId={Uri.EscapeDataString(accountId)}&dateFrom={from:yyyy-MM-dd}";
            var paginas = 0;

            while (!string.IsNullOrEmpty(query) && paginas++ < 50)
            {
                using var resp = await Send(HttpMethod.Get, "v2/transactions" + query);
                await GarantirSucesso(resp, "listar transações");
                var page = await resp.Content.ReadFromJsonAsync<PluggyCursorPage<PluggyApiTransaction>>(_json);
                if (page == null) break;
                todas.AddRange(page.Results);
                query = page.Next;
            }

            return todas;
        }
    }
}
