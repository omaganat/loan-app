using System.Text.RegularExpressions;

namespace LoanApp.Domain;

public static class Ssn
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, @"\D", string.Empty);
    }

    public static bool IsValid(string normalized) =>
        normalized.Length == 9 && normalized.All(char.IsDigit);

    public static string Format(string normalized)
    {
        if (!IsValid(normalized))
        {
            return normalized;
        }

        return $"{normalized[..3]}-{normalized[3..5]}-{normalized[5..]}";
    }
}
