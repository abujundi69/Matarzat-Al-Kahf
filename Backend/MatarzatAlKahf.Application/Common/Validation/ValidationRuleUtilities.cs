using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Common.Validation;

internal static class ValidationRuleUtilities
{
    public static void ValidateRequiredLanguages(
        IEnumerable<LanguageCode> languages,
        string field,
        Dictionary<string, List<string>> errors)
    {
        LanguageCode[] values = languages.ToArray();

        foreach (IGrouping<LanguageCode, LanguageCode> duplicate in
                 values.GroupBy(language => language).Where(group => group.Count() > 1))
        {
            Add(errors, field, $"Language '{duplicate.Key}' can appear only once.");
        }

        if (values.Count(language => language == LanguageCode.Arabic) != 1)
        {
            Add(errors, field, "Exactly one Arabic translation is required.");
        }

        if (values.Count(language => language == LanguageCode.English) != 1)
        {
            Add(errors, field, "Exactly one English translation is required.");
        }

        if (values.Any(language => !Enum.IsDefined(language)))
        {
            Add(errors, field, "Translation language is not supported.");
        }
    }

    public static void AddIf(
        Dictionary<string, List<string>> errors,
        bool condition,
        string field,
        string error)
    {
        if (condition)
        {
            Add(errors, field, error);
        }
    }

    public static void Add(
        Dictionary<string, List<string>> errors,
        string field,
        string error)
    {
        if (!errors.TryGetValue(field, out List<string>? fieldErrors))
        {
            fieldErrors = [];
            errors[field] = fieldErrors;
        }

        fieldErrors.Add(error);
    }

    public static void ThrowIfAny(
        Dictionary<string, List<string>> errors,
        string message)
    {
        if (errors.Count == 0)
        {
            return;
        }

        throw new ValidationException(
            message,
            errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase));
    }
}
