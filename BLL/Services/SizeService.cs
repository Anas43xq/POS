using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;

public class SizeService : ISizeService
{
    private readonly ISizeRepository _repo;

    public SizeService(ISizeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<SizeDto>>> GetAllSizesAsync()
    {
        var sizes = await _repo.GetAllOrderedAsync();
        var dtos = sizes.Select(MapToDto).ToList();
        return Result<List<SizeDto>>.Success(dtos);
    }

    public async Task<SizeDto?> GetSizeByIdAsync(int id)
    {
        var size = await _repo.GetByIdAsync(id);
        return size is null ? null : MapToDto(size);
    }

    public async Task AddSizeAsync(SizeDto size)
    {
        var entity = new Size
        {
            Name = size.Name,
            DisplayOrder = size.DisplayOrder,
            IsActive = size.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(entity);
    }

    public async Task UpdateSizeAsync(SizeDto size)
    {
        var existing = await _repo.GetByIdAsync(size.SizeId);
        if (existing is null) return;

        existing.Name = size.Name;
        existing.DisplayOrder = size.DisplayOrder;
        existing.IsActive = size.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing);
    }

    public async Task DeleteSizeAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }

    private static SizeDto MapToDto(Size s) => new()
    {
        SizeId = s.SizeId,
        Name = s.Name,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive
    };
}