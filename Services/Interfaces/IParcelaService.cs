﻿using ContaMente.DTOs;
using ContaMente.Models;

namespace ContaMente.Services.Interfaces
{
    public interface IParcelaService
    {
        Task<List<Parcela>> GetParcelas(string userId);
        Task<Parcela?> GetParcelaById(int id, string userId);
        Task<Parcela> CreateParcela(CreateParcelaDto createParcelaDto);
        Task<Parcela?> UpdateParcela(int id, CreateParcelaDto createParcelaDto, string userId);
        Task<bool> DeleteParcela(int id, string userId);
    }
}
