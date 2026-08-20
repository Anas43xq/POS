using BLL.DTOs;
using BLL.Interfaces;
using Contracts.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllCategoriesWithChildrenAsync();

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        var response = result.Value!.Select(MapToResponse).ToList();

        return Ok(response);
    }

    private static CategoryResponse MapToResponse(CategoryDto c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        Description = c.Description,
        ProductCount = c.ProductCount,
        ChildCategories = c.ChildCategories.Select(MapToResponse).ToList()
    };
}
