using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DAL.Entities;

namespace UI.ViewModels
{
    public partial class ProductManagementViewModel
    {
        private async Task LoadDataAsync()
        {
            var categoriesResult = await _categoryService.GetAllCategoriesWithChildrenAsync();
            if (categoriesResult.IsSuccess && categoriesResult.Value != null)
            {
                _allCategoryNodes.Clear();
                foreach (var c in categoriesResult.Value)
                {
                    _allCategoryNodes.Add(ToCategoryNode(c));
                }
            }

            var productsResult = await _productService.GetAllProductsAsync();
            if (productsResult.IsSuccess && productsResult.Value != null)
            {
                _allProducts.Clear();
                foreach (var p in productsResult.Value)
                {
                    _allProducts.Add(ToProductRow(p));
                }
            }

            ApplyCategoryFilter();
            ApplyProductFilter();
            SelectedCategory = null;
        }

        private CategoryNodeViewModel ToCategoryNode(Category category)
        {
            var node = new CategoryNodeViewModel
            {
                Id = category.CategoryId,
                Name = category.Name,
                ParentId = category.ParentCategoryId,
                Icon = string.IsNullOrWhiteSpace(category.Description) ? "📁" : (category.Description.StartsWith("http") || category.Description.Length <= 2 ? category.Description : "📁"),
                IsRoot = category.ParentCategoryId == null
            };

            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories)
                {
                    node.Children.Add(ToCategoryNode(child));
                }
            }

            return node;
        }

        private ProductRowViewModel ToProductRow(Product product)
        {
            return new ProductRowViewModel
            {
                Id = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.ParentCategoryId == null && product.Category != null
                    ? product.Category.Name
                    : (product.Category?.ParentCategory?.Name ?? string.Empty),
                Price = product.UnitPrice,
                TaxRateId = product.TaxRateId,
                TaxRateName = product.TaxRate?.Name ?? string.Empty,
                Status = product.IsActive ? "Active" : "Inactive"
            };
        }

        private void ApplyCategoryFilter()
        {
            CategoryRoots.Clear();

            var query = CategorySearchText.Trim();
            var sorted = _allCategoryNodes
                .Where(c => c.Children.Any())
                .OrderBy(c => c.Name)
                .ToList();

            if (string.IsNullOrWhiteSpace(query))
            {
                foreach (var root in sorted)
                {
                    CategoryRoots.Add(root);
                }
                return;
            }

            foreach (var root in sorted)
            {
                var filteredRoot = BuildFilteredBranch(root, query);
                if (filteredRoot != null)
                {
                    CategoryRoots.Add(filteredRoot);
                }
            }
        }

        private CategoryNodeViewModel? BuildFilteredBranch(CategoryNodeViewModel node, string query)
        {
            var children = node.Children
                .Select(child => BuildFilteredBranch(child, query))
                .Where(child => child != null)
                .Cast<CategoryNodeViewModel>()
                .ToList();

            var isMatch = node.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || children.Count > 0;
            if (!isMatch)
            {
                return null;
            }

            bool shouldExpand = children.Count > 0;

            return new CategoryNodeViewModel
            {
                Id = node.Id,
                Name = node.Name,
                ParentId = node.ParentId,
                Icon = node.Icon,
                IsRoot = node.IsRoot,
                IsExpanded = shouldExpand,
                Children = new ObservableCollection<CategoryNodeViewModel>(children)
            };
        }

        private void AddChildCategoryIds(CategoryNodeViewModel node, HashSet<int> ids)
        {
            foreach (var child in node.Children)
            {
                ids.Add(child.Id);
                AddChildCategoryIds(child, ids);
            }
        }

        private void ApplyProductFilter()
        {
            Products.Clear();

            var query = ProductSearchText.Trim();
            var selectedCategoryIds = new HashSet<int>();
            if (SelectedCategory != null && SelectedCategory.Id >= 0)
            {
                selectedCategoryIds.Add(SelectedCategory.Id);
                AddChildCategoryIds(SelectedCategory, selectedCategoryIds);
            }

            var filtered = _allProducts.Where(product =>
            {
                var matchesText = string.IsNullOrWhiteSpace(query) ||
                    product.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

                var matchesCategory = SelectedCategory == null || SelectedCategory.Id < 0 || selectedCategoryIds.Contains(product.CategoryId);

                return matchesText && matchesCategory;
            });

            foreach (var product in filtered.OrderBy(p => p.Name))
            {
                Products.Add(product);
            }

            if (SelectedProduct != null && !Products.Contains(SelectedProduct))
            {
                SelectedProduct = null;
            }
        }
    }
}
