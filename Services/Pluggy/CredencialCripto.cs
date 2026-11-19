using System.Security.Cryptography;
using System.Text;

namespace ContaMente.Services.Pluggy
{
    // Criptografa segredos guardados no banco com AES-256-GCM.
    // A chave vem de CREDENCIAIS_CHAVE (32 bytes em base64; gere com `openssl rand -base64 32`).
    // Se a chave for trocada, os segredos salvos deixam de abrir e precisam ser cadastrados de novo.
    public static class CredencialCripto
    {
        private const int TamanhoNonce = 12;
        private const int TamanhoTag = 16;

        private static byte[] Chave()
        {
            var valor = Environment.GetEnvironmentVariable("CREDENCIAIS_CHAVE");
            if (string.IsNullOrWhiteSpace(valor))
                throw new InvalidOperationException("CREDENCIAIS_CHAVE não configurada no servidor.");

            byte[] chave;
            try { chave = Convert.FromBase64String(valor.Trim()); }
            catch (FormatException) { throw new InvalidOperationException("CREDENCIAIS_CHAVE deve estar em base64."); }

            if (chave.Length != 32)
                throw new InvalidOperationException("CREDENCIAIS_CHAVE deve ter 32 bytes (openssl rand -base64 32).");
            return chave;
        }

        public static string Criptografar(string texto)
        {
            var dados = Encoding.UTF8.GetBytes(texto);
            var nonce = RandomNumberGenerator.GetBytes(TamanhoNonce);
            var tag = new byte[TamanhoTag];
            var cifrado = new byte[dados.Length];

            using var aes = new AesGcm(Chave(), TamanhoTag);
            aes.Encrypt(nonce, dados, cifrado, tag);

            return Convert.ToBase64String([.. nonce, .. tag, .. cifrado]);
        }

        public static string Descriptografar(string valor)
        {
            var bytes = Convert.FromBase64String(valor);
            var nonce = bytes.AsSpan(0, TamanhoNonce);
            var tag = bytes.AsSpan(TamanhoNonce, TamanhoTag);
            var cifrado = bytes.AsSpan(TamanhoNonce + TamanhoTag);
            var dados = new byte[cifrado.Length];

            using var aes = new AesGcm(Chave(), TamanhoTag);
            aes.Decrypt(nonce, cifrado, tag, dados);

            return Encoding.UTF8.GetString(dados);
        }
    }
}
