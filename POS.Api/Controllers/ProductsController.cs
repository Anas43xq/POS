using BLL.Interfaces;
using Contracts.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsWithVariantsAsync();

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        var response = result.Value!.Select(p => new ProductResponse
        {
            ProductId = p.ProductId,
            Name = p.Name,
            CategoryId = p.CategoryId,
            CategoryName = p.CategoryName,
            TaxRateId = p.TaxRateId,
            TaxRateName = p.TaxRateName,
            TaxRatePercentage = p.TaxRatePercentage,
            IsActive = p.IsActive,
            Variants = p.Variants.Select(v => new ProductVariantResponse
            {
                VariantId = v.VariantId,
                Size = v.SizeName,
                UnitPrice = v.UnitPrice
            }).ToList()
        }).ToList();

        return Ok(response);
    }
}