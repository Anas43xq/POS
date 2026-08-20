using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class SizeService : ISizeService
{
    private readonly ISizeRepository _repo;
    private readonly ILogger<SizeService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly ISessionService _sessionService;

    public SizeService(ISizeRepository repo, ILogger<SizeService> logger, IAuditLogService auditLogService, ISessionService sessionService)
    {
        _repo = repo;
        _logger = logger;
        _auditLogService = auditLogService;
        _sessionService = sessionService;
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

    public async Task<Result<bool>> AddSizeAsync(SizeDto size)
    {
        if (string.IsNullOrWhiteSpace(size.Name))
            return Result<bool>.Failure("Size name is required.");

        try
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
            await _auditLogService.LogAsync("Create", "Size", entity.SizeId, _sessionService.CurrentUser?.UserId,
                oldValue: null, newValue: MapToDto(entity));
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            _logger.LogError(ex, "Failed to add size due to SQL error {Number}", sqlEx.Number);
            return Result<bool>.Failure(TranslateSqlException(sqlEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add size");
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateSizeAsync(SizeDto size)
    {
        if (string.IsNullOrWhiteSpace(size.Name))
            return Result<bool>.Failure("Size name is required.");

        try
        {
            var existing = await _repo.GetByIdAsync(size.SizeId);
            if (existing is null)
                return Result<bool>.Failure($"Size {size.SizeId} not found.");

            var before = MapToDto(existing);

            existing.Name = size.Name;
            existing.DisplayOrder = size.DisplayOrder;
            existing.IsActive = size.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(existing);
            await _auditLogService.LogAsync("Update", "Size", existing.SizeId, _sessionService.CurrentUser?.UserId,
                oldValue: before, newValue: MapToDto(existing));
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            _logger.LogError(ex, "Failed to update size {SizeId} due to SQL error {Number}", size.SizeId, sqlEx.Number);
            return Result<bool>.Failure(TranslateSqlException(sqlEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update size {SizeId}", size.SizeId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteSizeAsync(int id)
    {
        try
        {
            var before = await _repo.GetByIdAsync(id);
            await _repo.DeleteAsync(id);
            await _auditLogService.LogAsync("Delete", "Size", id, _sessionService.CurrentUser?.UserId,
                oldValue: before is null ? null : MapToDto(before), newValue: null);
            return Result<bool>.Success(true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            _logger.LogError(ex, "Failed to delete size {SizeId} due to SQL error {Number}", id, sqlEx.Number);
            return Result<bool>.Failure(TranslateSqlException(sqlEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete size {SizeId}", id);
            return Result<bool>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Translates SQL Server errors that can surface from Size add/update/
    /// delete (most notably a foreign-key violation when deleting a size
    /// that's still referenced by one or more product variants) into a
    /// friendly message, rather than leaking raw SQL Server text to the UI.
    /// </summary>
    private static string TranslateSqlException(SqlException ex)
    {
        if (ex.Number == 547)
            return "Cannot delete size: it is in use by one or more products.";

        return "A database error occurred while saving the size.";
    }

    private static SizeDto MapToDto(Size s) => new()
    {
        SizeId = s.SizeId,
        Name = s.Name,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive
    };
}