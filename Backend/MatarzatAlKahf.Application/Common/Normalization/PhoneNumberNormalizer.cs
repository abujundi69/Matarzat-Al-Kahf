using System.Globalization;
using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Normalization;

public sealed class PhoneNumberNormalizer : IPhoneNumberNormalizer
{
    public string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw ValidationException.For(nameof(value), "Phone number is required.");
        }

        var digits = new StringBuilder(13);
        foreach (Rune rune in value.Trim().EnumerateRunes())
        {
            if (rune.Value is >= '0' and <= '9')
            {
                digits.Append((char)rune.Value);
            }
            else if (rune.Value != '+' && !IsFormattingCharacter(rune))
            {
                throw ValidationException.For(nameof(value), "Phone number contains unsupported characters.");
            }
        }

        string national = digits.ToString() switch
        {
            var number when number.StartsWith("00970", StringComparison.Ordinal) => number[5..],
            var number when number.StartsWith("970", StringComparison.Ordinal) => number[3..],
            var number when number.StartsWith('0') => number[1..],
            var number => number
        };

        if (national.Length != 9
            || !(national.StartsWith("59", StringComparison.Ordinal)
                || national.StartsWith("56", StringComparison.Ordinal)))
        {
            throw ValidationException.For(
                nameof(value),
                "Enter a valid Palestinian mobile number (059/056 or +97059/+97056)." );
        }

        return "+970" + national;
    }

    private static bool IsFormattingCharacter(Rune rune)
    {
        UnicodeCategory category = Rune.GetUnicodeCategory(rune);
        return Rune.IsWhiteSpace(rune)
            || category is UnicodeCategory.DashPunctuation
                or UnicodeCategory.OpenPunctuation
                or UnicodeCategory.ClosePunctuation
                or UnicodeCategory.Format
            || rune.Value == '.';
    }
}
