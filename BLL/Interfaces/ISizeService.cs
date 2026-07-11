using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces;

public interface ISizeService
{
    Task<Result<List<SizeDto>>> GetAllSizesAsync();

    Task<SizeDto?> GetSizeByIdAsync(int id);

    Task AddSizeAsync(SizeDto size);

    Task UpdateSizeAsync(SizeDto size);

    Task DeleteSizeAsync(int id);
}