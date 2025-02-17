using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
using Hangfire;
using System.Formats.Asn1;

namespace ContaMente.Services
{
    public class RecorrenciaService : IRecorrenciaService
    {
        private readonly IRecorrenciaRepository _recorrenciaRepository;
        public RecorrenciaService(IRecorrenciaRepository recorrencia) 
        {
            _recorrenciaRepository = recorrencia;
        }

        public async Task<Recorrencia?> GetRecorrenciaById(int id)
        {
            return await _recorrenciaRepository.GetRecorrenciaById(id);
        }

        public async Task<Recorrencia> CreateRecorrencia(Recorrencia recorrencia)
        {
            return await _recorrenciaRepository.CreateRecorrencia(recorrencia);
        }

        public async Task CancelarRecorrencia(int recorrenciaId, string userId)
        {
            var recorrencia = await _recorrenciaRepository.GetRecorrenciaById(recorrenciaId);

            if (recorrencia != null)
            {
                var pertenceAoUsuario = recorrencia.Movimentacoes.Any(m => m.Categoria!.UserId == userId);

                if (pertenceAoUsuario)
                {
                    recorrencia.DataFim = DateTime.UtcNow;
                    await _recorrenciaRepository.UpdateRecorrencia(recorrencia);

                    RecurringJob.RemoveIfExists($"recorrencia_{recorrencia.Id}");
                }
                else
                {
                    throw new UnauthorizedAccessException("O usuário não tem permissão para cancelar esta recorrência.");
                }
            }
        }
    }
}
