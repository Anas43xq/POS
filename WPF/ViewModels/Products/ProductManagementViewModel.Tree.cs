using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using BLL.DTOs;

namespace UI.ViewModels
{
    public partial class ProductManagementViewModel
    {
        private void RestartSearchDebounce()
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void OnSearchDebounceTick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            ApplyCategoryFilter();
            ApplyProductFilter();
        }

        private void CacheCategoryIds()
        {
            if (SelectedCategory != null && SelectedCategory.Id >= 0)
            {
                _cachedCategoryIds = new HashSet<int> { SelectedCategory.Id };
                AddChildCategoryIds(SelectedCategory, _cachedCategoryIds);
            }
            else
            {
                _cachedCategoryIds = null;
            }
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Loads categories and products for the current language.
        /// <para>
        /// The BLL/DAL calls this method awaits (<c>GetAllCategoriesWithChildrenAsync</c>,
        /// <c>GetAllProductsAsync</c>) don't accept a <see cref="CancellationToken"/>,
        /// so this can't cancel in-flight I/O. What it does guard against is
        /// a rapid double-trigger (e.g. the user switches language twice in
        /// quick succession, firing <see cref="OnLanguageChanged"/> twice):
        /// each call cancels the token belonging to any still-running
        /// previous call, and that previous call checks the token after
        /// each await before touching VM state — so if two loads overlap,
        /// only the most recent one is allowed to apply its results.
        /// </para>
        /// </summary>
        private async Task LoadDataAsync()
        {
            _loadCts?.Cancel();
            var cts = new CancellationTokenSource();
            _loadCts = cts;

            var languageCode = _localization.CurrentLanguage.FilePrefix;

            var categoriesResult = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
            if (cts.Token.IsCancellationRequested)
                return;

            if (categoriesResult.IsSuccess && categoriesResult.Value != null)
            {
                _allCategoryNodes.Clear();
                foreach (var c in categoriesResult.Value)
                {
                    _allCategoryNodes.Add(ToCategoryNode(c));
                }
            }

            var productsResult = await _productService.GetAllProductsAsync(languageCode);
            if (cts.Token.IsCancellationRequested)
                return;

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
            OnPropertyChanged(nameof(IsCategorySearchEmpty));
            OnPropertyChanged(nameof(IsProductSearchEmpty));
        }

        private CategoryNodeViewModel ToCategoryNode(CategoryDto category)
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

        private ProductRowViewModel ToProductRow(ProductSummaryDto product)
        {
            return new ProductRowViewModel
            {
                Id = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = FindCategoryNodeById(product.CategoryId)?.Name ?? string.Empty,
                MinPrice = product.MinUnitPrice,
                MaxPrice = product.MaxUnitPrice,
                VariantCount = product.VariantCount,
                TaxRateId = product.TaxRateId,
                TaxRateName = product.TaxRateName,
                Status = product.IsActive ? "Active" : "Inactive"
            };
        }

        /// <summary>
        /// Depth-first lookup of a category by id across the whole loaded
        /// tree (_allCategoryNodes holds only the roots, with children
        /// nested underneath). Used to show a product's direct parent
        /// category — e.g. "Manakeesh" — rather than its root ("Food") or
        /// nothing at all.
        /// </summary>
        private CategoryNodeViewModel? FindCategoryNodeById(int categoryId)
        {
            foreach (var root in _allCategoryNodes)
            {
                var match = FindCategoryNodeById(root, categoryId);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static CategoryNodeViewModel? FindCategoryNodeById(CategoryNodeViewModel node, int categoryId)
        {
            if (node.Id == categoryId)
                return node;

            foreach (var child in node.Children)
            {
                var match = FindCategoryNodeById(child, categoryId);
                if (match != null)
                    return match;
            }

            return null;
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
                OnPropertyChanged(nameof(IsCategorySearchEmpty));
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

            OnPropertyChanged(nameof(IsCategorySearchEmpty));
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
            Products.Refresh();

            if (SelectedProduct != null && !Products.Cast<ProductRowViewModel>().Contains(SelectedProduct))
            {
                SelectedProduct = null;
            }

            OnPropertyChanged(nameof(IsProductSearchEmpty));
        }

        private bool FilterProduct(object obj)
        {
            if (obj is not ProductRowViewModel product)
                return false;

            var query = ProductSearchText.Trim();

            var matchesText = string.IsNullOrWhiteSpace(query) ||
                product.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

            var matchesCategory = _cachedCategoryIds == null || _cachedCategoryIds.Contains(product.CategoryId);

            return matchesText && matchesCategory;
        }
    }
}
