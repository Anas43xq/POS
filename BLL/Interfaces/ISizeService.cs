using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces;

public interface ISizeService
{
    Task<Result<List<SizeDto>>> GetAllSizesAsync();

    Task<SizeDto?> GetSizeByIdAsync(int id);

    Task<Result<bool>> AddSizeAsync(SizeDto size);

    Task<Result<bool>> UpdateSizeAsync(SizeDto size);

    Task<Result<bool>> DeleteSizeAsync(int id);
}