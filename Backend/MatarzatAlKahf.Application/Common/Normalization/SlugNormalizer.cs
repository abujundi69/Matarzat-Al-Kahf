using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Normalization;

public sealed class SlugNormalizer : ISlugNormalizer
{
    public string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationException.For(nameof(value), "Slug is required.");
        }

        var result = new StringBuilder(value.Length);
        bool separatorPending = false;

        foreach (Rune rune in value.Trim().EnumerateRunes())
        {
            if (IsAllowed(rune))
            {
                if (separatorPending && result.Length > 0)
                {
                    result.Append('-');
                }

                AppendNormalized(result, rune);
                separatorPending = false;
            }
            else if (Rune.IsWhiteSpace(rune) || rune.Value == '-')
            {
                separatorPending = result.Length > 0;
            }
        }

        if (result.Length == 0)
        {
            throw ValidationException.For(nameof(value), "Slug contains no valid characters.");
        }

        return result.ToString();
    }

    private static bool IsAllowed(Rune rune)
    {
        int value = rune.Value;
        bool isEnglishLetter = value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        bool isNumber = value is >= '0' and <= '9';

        return isEnglishLetter || isNumber || IsArabicLetter(rune);
    }

    private static bool IsArabicLetter(Rune rune)
    {
        int value = rune.Value;
        bool isArabicBlock = value is >= 0x0600 and <= 0x06FF
            or >= 0x0750 and <= 0x077F
            or >= 0x0870 and <= 0x089F
            or >= 0x08A0 and <= 0x08FF
            or >= 0xFB50 and <= 0xFDFF
            or >= 0xFE70 and <= 0xFEFF;

        return isArabicBlock && Rune.IsLetter(rune);
    }

    private static void AppendNormalized(StringBuilder builder, Rune rune)
    {
        int value = rune.Value;
        if (value is >= 'A' and <= 'Z')
        {
            builder.Append((char)(value + ('a' - 'A')));
            return;
        }

        builder.Append(rune.ToString());
    }
}
