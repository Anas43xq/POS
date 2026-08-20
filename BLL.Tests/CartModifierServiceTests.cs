using BLL.DTOs;
using BLL.Models;
using BLL.Services;
using FluentAssertions;
using Xunit;

namespace BLL.Tests;

public sealed class CartModifierServiceTests
{
    private readonly CartModifierService _sut = new();

    // ═══════════════════════════════════════════════════════════════════
    // ValidateModifierSelections
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ValidateModifierSelections_RequiredGroupWithNoSelection_ReturnsError()
    {
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 1, Name = "Dough", IsRequired = true, GroupType = 1, MinSelections = 1, MaxSelections = 1 },
        };
        var selected = new List<CartItemModifier>();

        var errors = _sut.ValidateModifierSelections(groups, selected);

        errors.Should().ContainSingle()
              .Which.Should().Contain("Dough").And.Contain("required");
    }

    [Fact]
    public void ValidateModifierSelections_RequiredGroupWithSelection_ReturnsNoErrors()
    {
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 1, Name = "Dough", IsRequired = true, GroupType = 1, MinSelections = 1, MaxSelections = 1 },
        };
        var selected = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 1, ModifierOptionId = 10, OptionName = "Thin Crust" },
        };

        var errors = _sut.ValidateModifierSelections(groups, selected);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateModifierSelections_BelowMinSelections_ReturnsError()
    {
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 2, Name = "Toppings", IsRequired = false, GroupType = 2, MinSelections = 2, MaxSelections = 5 },
        };
        var selected = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 2, ModifierOptionId = 20 },
        };

        var errors = _sut.ValidateModifierSelections(groups, selected);

        errors.Should().ContainSingle()
              .Which.Should().Contain("select at least 2");
    }

    [Fact]
    public void ValidateModifierSelections_AboveMaxSelections_ReturnsError()
    {
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 2, Name = "Toppings", IsRequired = false, GroupType = 2, MinSelections = 1, MaxSelections = 2 },
        };
        var selected = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 2, ModifierOptionId = 20 },
            new() { ModifierGroupId = 2, ModifierOptionId = 21 },
            new() { ModifierGroupId = 2, ModifierOptionId = 22 },
        };

        var errors = _sut.ValidateModifierSelections(groups, selected);

        errors.Should().ContainSingle()
              .Which.Should().Contain("select at most 2");
    }

    [Fact]
    public void ValidateModifierSelections_QuantityGroupType3_SkipsMinMaxValidation()
    {
        // GroupType == 3 is explicitly exempted from min/max checks.
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 3, Name = "Extra Cheese", IsRequired = false, GroupType = 3, MinSelections = 1, MaxSelections = 1 },
        };
        var selected = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 3, ModifierOptionId = 30, Quantity = 5 },
            new() { ModifierGroupId = 3, ModifierOptionId = 31, Quantity = 2 },
        };

        var errors = _sut.ValidateModifierSelections(groups, selected);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateModifierSelections_BothRequiredAndMinMax_ReturnsMultipleErrors()
    {
        var groups = new List<ModifierGroupDto>
        {
            new() { ModifierGroupId = 1, Name = "Dough", IsRequired = true, GroupType = 1, MinSelections = 1, MaxSelections = 1 },
            new() { ModifierGroupId = 2, Name = "Toppings", IsRequired = false, GroupType = 2, MinSelections = 2, MaxSelections = 5 },
        };

        var errors = _sut.ValidateModifierSelections(groups, new List<CartItemModifier>());

        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Contains("required"));
        errors.Should().Contain(e => e.Contains("select at least"));
    }

    // ═══════════════════════════════════════════════════════════════════
    // ApplyModifier
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void ApplyModifier_SingleSelectGroup_ReplacesExistingSelection()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 1, Name = "Dough", GroupType = 1 };
        var option = new ModifierOptionDto { ModifierOptionId = 10, Name = "Thin Crust", PriceAdd = 2.0m };
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 1, ModifierOptionId = 99, OptionName = "Thick" },
        };

        _sut.ApplyModifier(group, option, 1, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.ModifierOptionId.Should().Be(10);
    }

    [Fact]
    public void ApplyModifier_SingleSelectGroup_AddsFirstSelection()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 1, Name = "Dough", GroupType = 1 };
        var option = new ModifierOptionDto { ModifierOptionId = 10, Name = "Thin Crust", PriceAdd = 2.0m };
        var modifiers = new List<CartItemModifier>();

        _sut.ApplyModifier(group, option, 1, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.ModifierOptionId.Should().Be(10);
    }

    [Fact]
    public void ApplyModifier_MultiSelectGroup_TogglesExistingOptionOff()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 2, Name = "Toppings", GroupType = 2 };
        var option = new ModifierOptionDto { ModifierOptionId = 20, Name = "Mushrooms", PriceAdd = 1.5m };
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 2, ModifierOptionId = 20, OptionName = "Mushrooms" },
            new() { ModifierGroupId = 2, ModifierOptionId = 21, OptionName = "Olives" },
        };

        _sut.ApplyModifier(group, option, 1, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.ModifierOptionId.Should().Be(21);
    }

    [Fact]
    public void ApplyModifier_MultiSelectGroup_AddsNewDistinctOption()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 2, Name = "Toppings", GroupType = 2 };
        var option = new ModifierOptionDto { ModifierOptionId = 22, Name = "Pepperoni", PriceAdd = 2.0m };
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 2, ModifierOptionId = 20, OptionName = "Mushrooms" },
        };

        _sut.ApplyModifier(group, option, 1, modifiers);

        modifiers.Should().HaveCount(2);
        modifiers.Should().Contain(m => m.ModifierOptionId == 22);
    }

    [Fact]
    public void ApplyModifier_QuantityGroup_IncrementsQuantity()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 3, Name = "Extra Cheese", GroupType = 3 };
        var option = new ModifierOptionDto { ModifierOptionId = 30, Name = "Mozzarella", PriceAdd = 1.0m };
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierGroupId = 3, ModifierOptionId = 30, OptionName = "Mozzarella", Quantity = 2, PriceAdd = 1.0m },
        };

        _sut.ApplyModifier(group, option, 3, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.Quantity.Should().Be(5); // 2 + 3
    }

    [Fact]
    public void ApplyModifier_QuantityGroup_AddsNewOptionWhenNotPresent()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 3, Name = "Sides", GroupType = 3 };
        var option = new ModifierOptionDto { ModifierOptionId = 31, Name = "Ranch Dip", PriceAdd = 0.5m };
        var modifiers = new List<CartItemModifier>();

        _sut.ApplyModifier(group, option, 2, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.Quantity.Should().Be(2);
    }

    [Fact]
    public void ApplyModifier_QuantityZero_ClampedToOne()
    {
        var group = new ModifierGroupDto { ModifierGroupId = 3, Name = "Sauces", GroupType = 3 };
        var option = new ModifierOptionDto { ModifierOptionId = 40, Name = "Ketchup", PriceAdd = 0 };
        var modifiers = new List<CartItemModifier>();

        _sut.ApplyModifier(group, option, 0, modifiers);

        modifiers.Should().ContainSingle()
                 .Which.Quantity.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════════
    // RemoveModifier
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void RemoveModifier_MatchingOptionId_RemovesOnlyThatOption()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 10, OptionName = "Thin Crust" },
            new() { ModifierOptionId = 20, OptionName = "Mushrooms" },
            new() { ModifierOptionId = 21, OptionName = "Olives" },
        };

        _sut.RemoveModifier(20, modifiers);

        modifiers.Should().HaveCount(2);
        modifiers.Should().NotContain(m => m.ModifierOptionId == 20);
    }

    [Fact]
    public void RemoveModifier_NonMatchingOptionId_LeavesListUnchanged()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { ModifierOptionId = 10 },
            new() { ModifierOptionId = 20 },
        };

        _sut.RemoveModifier(99, modifiers);

        modifiers.Should().HaveCount(2);
    }

    // ═══════════════════════════════════════════════════════════════════
    // CalculateModifierTotal
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void CalculateModifierTotal_EmptyList_ReturnsZero()
    {
        _sut.CalculateModifierTotal(new List<CartItemModifier>()).Should().Be(0);
    }

    [Fact]
    public void CalculateModifierTotal_SumsLineTotals()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { PriceAdd = 2.0m, Quantity = 2 },   // LineTotal = 4.00
            new() { PriceAdd = 1.5m, Quantity = 1 },   // LineTotal = 1.50
        };

        _sut.CalculateModifierTotal(modifiers).Should().Be(5.50m);
    }

    // ═══════════════════════════════════════════════════════════════════
    // BuildModifierSummary
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void BuildModifierSummary_AllDefault_IncludesAllOptions()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { OptionName = "Regular Dough", IsDefault = true },
            new() { OptionName = "No Extra", IsDefault = true },
        };

        _sut.BuildModifierSummary(modifiers).Should().Be("Regular Dough, No Extra");
    }

    [Fact]
    public void BuildModifierSummary_SingleNonDefault_ReturnsOptionNameOnly()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { OptionName = "Thin Crust", IsDefault = false, Quantity = 1 },
        };

        _sut.BuildModifierSummary(modifiers).Should().Be("Thin Crust");
    }

    [Fact]
    public void BuildModifierSummary_MultipleNonDefaults_JoinsWithComma()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { OptionName = "Thin Crust", IsDefault = false, Quantity = 1 },
            new() { OptionName = "Extra Cheese", IsDefault = false, Quantity = 1 },
        };

        var summary = _sut.BuildModifierSummary(modifiers);

        summary.Should().Be("Thin Crust, Extra Cheese");
    }

    [Fact]
    public void BuildModifierSummary_QuantityGreaterThanOne_AppendsMultiplier()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { OptionName = "Mozzarella", IsDefault = false, Quantity = 3 },
        };

        _sut.BuildModifierSummary(modifiers).Should().Be("Mozzarella ×3");
    }

    [Fact]
    public void BuildModifierSummary_MixedDefaultsAndCustomizations_IncludesAllOptions()
    {
        var modifiers = new List<CartItemModifier>
        {
            new() { OptionName = "Regular Dough", IsDefault = true },
            new() { OptionName = "Mushrooms", IsDefault = false, Quantity = 1 },
            new() { OptionName = "No Sauce", IsDefault = true },
            new() { OptionName = "Olives", IsDefault = false, Quantity = 2 },
        };

        var summary = _sut.BuildModifierSummary(modifiers);

        summary.Should().Be("Regular Dough, Mushrooms, No Sauce, Olives ×2");
    }
}