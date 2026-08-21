using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI.Controls;

public enum KeyHintBadgeStyle
{
    Default,
    OnBrand
}

public partial class KeyHint : UserControl
{
    // ── Key ──────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register(
            nameof(Key),
            typeof(string),
            typeof(KeyHint),
            new PropertyMetadata(string.Empty));

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    // ── BadgeStyle ────────────────────────────────────────────────────────────

    public static readonly DependencyProperty BadgeStyleProperty =
        DependencyProperty.Register(
            nameof(BadgeStyle),
            typeof(KeyHintBadgeStyle),
            typeof(KeyHint),
            new PropertyMetadata(KeyHintBadgeStyle.Default, OnBadgeStyleChanged));

    public KeyHintBadgeStyle BadgeStyle
    {
        get => (KeyHintBadgeStyle)GetValue(BadgeStyleProperty);
        set => SetValue(BadgeStyleProperty, value);
    }

    private static void OnBadgeStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyHint kh)
            kh.ApplyStyle((KeyHintBadgeStyle)e.NewValue);
    }

    // ── Computed brush read-only DPs ──────────────────────────────────────────

    private static readonly DependencyPropertyKey BadgeBackgroundKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeBackground), typeof(Brush), typeof(KeyHint), new PropertyMetadata(null));
    public static readonly DependencyProperty BadgeBackgroundProperty = BadgeBackgroundKey.DependencyProperty;
    public Brush BadgeBackground
    {
        get => (Brush)GetValue(BadgeBackgroundProperty);
        private set => SetValue(BadgeBackgroundKey, value);
    }

    private static readonly DependencyPropertyKey BadgeForegroundKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeForeground), typeof(Brush), typeof(KeyHint), new PropertyMetadata(null));
    public static readonly DependencyProperty BadgeForegroundProperty = BadgeForegroundKey.DependencyProperty;
    public Brush BadgeForeground
    {
        get => (Brush)GetValue(BadgeForegroundProperty);
        private set => SetValue(BadgeForegroundKey, value);
    }

    private static readonly DependencyPropertyKey BadgeBorderBrushKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeBorderBrush), typeof(Brush), typeof(KeyHint), new PropertyMetadata(null));
    public static readonly DependencyProperty BadgeBorderBrushProperty = BadgeBorderBrushKey.DependencyProperty;
    public Brush BadgeBorderBrush
    {
        get => (Brush)GetValue(BadgeBorderBrushProperty);
        private set => SetValue(BadgeBorderBrushKey, value);
    }

    private static readonly DependencyPropertyKey BadgeBorderThicknessKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeBorderThickness), typeof(Thickness), typeof(KeyHint), new PropertyMetadata(new Thickness(0)));
    public static readonly DependencyProperty BadgeBorderThicknessProperty = BadgeBorderThicknessKey.DependencyProperty;
    public Thickness BadgeBorderThickness
    {
        get => (Thickness)GetValue(BadgeBorderThicknessProperty);
        private set => SetValue(BadgeBorderThicknessKey, value);
    }

    // ── OnBrand control-internal alpha constants ───────────────────────────────
    // These are justified exceptions: they are design-owned semi-transparent
    // overlays for use on brand-colored button backgrounds, not view-level hex.

    private static readonly Brush OnBrandBackground  = new SolidColorBrush(Color.FromArgb(0x28, 0xFF, 0xFF, 0xFF));
    private static readonly Brush OnBrandForeground  = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));

    static KeyHint()
    {
        OnBrandBackground.Freeze();
        OnBrandForeground.Freeze();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    public KeyHint()
    {
        InitializeComponent();
        ApplyStyle(BadgeStyle);
    }

    // ── Style application ─────────────────────────────────────────────────────

    private void ApplyStyle(KeyHintBadgeStyle style)
    {
        if (style == KeyHintBadgeStyle.OnBrand)
        {
            BadgeBackground     = OnBrandBackground;
            BadgeForeground     = OnBrandForeground;
            BadgeBorderBrush    = Brushes.Transparent;
            BadgeBorderThickness = new Thickness(0);
            return;
        }

        // Default — resolve from application token resources
        BadgeBackground     = ResolveResource("Color.Surface.Subtle",  Brushes.LightGray);
        BadgeForeground     = ResolveResource("Color.Text.Muted",      Brushes.DimGray);
        BadgeBorderBrush    = ResolveResource("Color.Border.Default",  Brushes.Gray);
        BadgeBorderThickness = new Thickness(1);
    }

    private static Brush ResolveResource(string key, Brush fallback)
    {
        try
        {
            if (Application.Current?.FindResource(key) is Brush b)
                return b;
        }
        catch (ResourceReferenceKeyNotFoundException) { }
        catch (Exception) { }
        return fallback;
    }
}
