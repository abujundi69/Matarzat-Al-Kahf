using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Normalization;

public sealed class SkuNormalizer : ISkuNormalizer
{
    public string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationException.For(nameof(value), "SKU is required.");
        }

        string normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 100)
        {
            throw ValidationException.For(nameof(value), "SKU cannot exceed 100 characters.");
        }

        return normalized;
    }
}
