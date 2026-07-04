using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productrepo;

        public ProductService(IProductRepository ProductRepo)
        {
            _productrepo = ProductRepo;
        }

        public async Task<Result<List<Product>>> GetAllProductsWithTaxRateAsync()
        {
            try
            {
                var products = await _productrepo.GetAllProductsWithTaxRateAsync();
                return Result<List<Product>>.Success(products.ToList());
            }
            catch (Exception ex)
            {
                return Result<List<Product>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<Product>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productrepo.GetAllProductsWithTaxRateAsync();
                return Result<List<Product>>.Success(products.ToList());
            }
            catch (Exception ex)
            {
                return Result<List<Product>>.Failure(ex.Message);
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id) =>
            await _productrepo.GetByIdAsync(id);

        public async Task AddProductAsync(Product Product) =>
            await _productrepo.AddAsync(Product);

        public async Task UpdateProductAsync(Product Product) =>
            await _productrepo.UpdateAsync(Product);

        public async Task DeleteProductAsync(int id) =>
            await _productrepo.DeleteAsync(id);
    }
}