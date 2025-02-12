using ContaMente.Models;
using ContaMente.Repositories.Interfaces;
using ContaMente.Services.Interfaces;
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
    }
}
