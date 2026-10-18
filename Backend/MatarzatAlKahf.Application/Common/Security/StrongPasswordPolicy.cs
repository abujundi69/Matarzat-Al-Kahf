using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Application.Common.Security;
public sealed class StrongPasswordPolicy : IPasswordPolicy
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;
    public void Validate(string password)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required.");
        }
        else
        {
            if (password.Length < MinimumLength || password.Length > MaximumLength)
            {
                errors.Add($"Password must be between {MinimumLength} and {MaximumLength} characters.");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Password must contain an uppercase letter.");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Password must contain a lowercase letter.");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Password must contain a number.");
            }

            if (!password.Any(character => !char.IsLetterOrDigit(character)))
            {
                errors.Add("Password must contain a non-alphanumeric character.");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Password does not satisfy the security policy.", new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { [nameof(password)] = errors.ToArray() });
        }
    }
}
