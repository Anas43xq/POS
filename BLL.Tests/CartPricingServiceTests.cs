using BLL.Models;
using BLL.Services;
using FluentAssertions;
using Xunit;

namespace BLL.Tests;

public sealed class CartPricingServiceTests
{
    private readonly CartPricingService _sut = new();

    // ═══════════════════════════════════════════════════════════════════
    // CalculateTotals
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void CalculateTotals_EmptyCart_ReturnsAllZero()
    {
        var totals = _sut.CalculateTotals(new List<CartItem>());

        totals.Subtotal.Should().Be(0);
        totals.Tax.Should().Be(0);
        totals.Total.Should().Be(0);
    }

    [Fact]
    public void CalculateTotals_SingleLine_SumsSubtotalTaxAndTotal()
    {
        var items = new List<CartItem>
        {
            new() { VariantId = 1, UnitPrice = 20.00m, Quantity = 1, TaxRate = 0.05m }
            // LineSubtotal = 20.00, LineTax = 1.00, LineTotal = 21.00
        };

        var totals = _sut.CalculateTotals(items);

        totals.Subtotal.Should().Be(20.00m);
        totals.Tax.Should().Be(1.00m);
        totals.Total.Should().Be(21.00m);
    }

    [Fact]
    public void CalculateTotals_MultipleLines_SumsAcrossLines()
    {
        var items = new List<CartItem>
        {
            new() { VariantId = 1, UnitPrice = 20.00m, Quantity = 2, TaxRate = 0.05m }, // Subtotal 40.00, Tax 2.00
            new() { VariantId = 2, UnitPrice = 10.00m, Quantity = 1, TaxRate = 0.10m }, // Subtotal 10.00, Tax 1.00
        };

        var totals = _sut.CalculateTotals(items);

        totals.Subtotal.Should().Be(50.00m);
        totals.Tax.Should().Be(3.00m);
        totals.Total.Should().Be(53.00m);
    }

    // ═══════════════════════════════════════════════════════════════════
    // FindMergeableLine
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void FindMergeableLine_NoExistingLines_ReturnsNull()
    {
        var result = _sut.FindMergeableLine(new List<CartItem>(), variantId: 1, modifiers: new List<CartItemModifier>());

        result.Should().BeNull();
    }

    [Fact]
    public void FindMergeableLine_SameVariantNoModifiers_ReturnsMatch()
    {
        var existing = new CartItem { VariantId = 1, Modifiers = new List<CartItemModifier>() };
        var items = new List<CartItem> { existing };

        var result = _sut.FindMergeableLine(items, variantId: 1, modifiers: new List<CartItemModifier>());

        result.Should().BeSameAs(existing);
    }

    [Fact]
    public void FindMergeableLine_DifferentVariant_ReturnsNull()
    {
        var existing = new CartItem { VariantId = 1, Modifiers = new List<CartItemModifier>() };
        var items = new List<CartItem> { existing };

        var result = _sut.FindMergeableLine(items, variantId: 2, modifiers: new List<CartItemModifier>());

        result.Should().BeNull();
    }

    [Fact]
    public void FindMergeableLine_SameVariantSameModifierSelection_ReturnsMatch()
    {
        var existing = new CartItem
        {
            VariantId = 1,
            Modifiers = new List<CartItemModifier>
            {
                new() { ModifierOptionId = 10, Quantity = 1 },
                new() { ModifierOptionId = 20, Quantity = 2 },
            }
        };
        var items = new List<CartItem> { existing };

        var incoming = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 20, Quantity = 2 },
            new() { ModifierOptionId = 10, Quantity = 1 }, // different order — should still match
        };

        var result = _sut.FindMergeableLine(items, variantId: 1, modifiers: incoming);

        result.Should().BeSameAs(existing);
    }

    [Fact]
    public void FindMergeableLine_SameVariantDifferentModifierSelection_ReturnsNull()
    {
        // Regression test for the audit finding: two lines can coincidentally
        // share the same UnitPrice while having genuinely different modifier
        // selections. Merging must key on the actual selection, not price.
        var existing = new CartItem
        {
            VariantId = 1,
            UnitPrice = 25.00m,
            Modifiers = new List<CartItemModifier>
            {
                new() { ModifierOptionId = 10, Quantity = 1 }, // e.g. "Large", PriceAdd = 5.00
            }
        };
        var items = new List<CartItem> { existing };

        // Same VariantId, same computed UnitPrice, but a different modifier
        // selection (e.g. "Oat Milk" instead of "Large", coincidentally
        // priced the same).
        var incoming = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 99, Quantity = 1 },
        };

        var result = _sut.FindMergeableLine(items, variantId: 1, modifiers: incoming);

        result.Should().BeNull();
    }

    [Fact]
    public void FindMergeableLine_SameOptionDifferentQuantity_ReturnsNull()
    {
        var existing = new CartItem
        {
            VariantId = 1,
            Modifiers = new List<CartItemModifier>
            {
                new() { ModifierOptionId = 30, Quantity = 1 }, // e.g. 1x Extra Shot
            }
        };
        var items = new List<CartItem> { existing };

        var incoming = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 30, Quantity = 2 }, // 2x Extra Shot — not the same line
        };

        var result = _sut.FindMergeableLine(items, variantId: 1, modifiers: incoming);

        result.Should().BeNull();
    }

    [Fact]
    public void FindMergeableLine_MultipleExistingLines_ReturnsOnlyExactMatch()
    {
        var vanillaLine = new CartItem
        {
            VariantId = 1,
            Modifiers = new List<CartItemModifier>
            {
                new() { ModifierOptionId = 10, Quantity = 1, IsDefault = true },
            }
        };
        var customizedLine = new CartItem
        {
            VariantId = 1,
            Modifiers = new List<CartItemModifier>
            {
                new() { ModifierOptionId = 20, Quantity = 1, IsDefault = false },
            }
        };
        var items = new List<CartItem> { vanillaLine, customizedLine };

        var incoming = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 20, Quantity = 1 },
        };

        var result = _sut.FindMergeableLine(items, variantId: 1, modifiers: incoming);

        result.Should().BeSameAs(customizedLine);
    }
}
