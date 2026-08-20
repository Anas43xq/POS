namespace BLL.Helpers;

public static class ProductNameFormatter
{
    public static string Format(string productName, string? sizeName)
    {
        if (string.IsNullOrWhiteSpace(sizeName))
            return productName;

        return $"{productName} - {sizeName}";
    }
}