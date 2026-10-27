using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Validation;

public static class TopicAnalysisValidator
{
    private static readonly string[] AllowedDifficulties =
    [
        "Beginner",
        "Intermediate",
        "Advanced"
    ];

    public static IReadOnlyList<string> Validate(TopicAnalysis? analysis)
    {
        if (analysis is null)
        {
            return ["Analysis is required."];
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(analysis.Summary))
        {
            errors.Add("Summary is required.");
        }
        else if (analysis.Summary.Length > 500)
        {
            errors.Add("Summary must be 500 characters or fewer.");
        }

        if (analysis.Technologies is null || analysis.Technologies.Count == 0)
        {
            errors.Add("At least one technology is required.");
        }
        else
        {
            if (analysis.Technologies.Count > 10)
            {
                errors.Add("Technologies must contain 10 items or fewer.");
            }

            if (analysis.Technologies.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add("Technologies cannot contain empty values.");
            }
        }

        if (string.IsNullOrWhiteSpace(analysis.Difficulty) ||
            !AllowedDifficulties.Any(value => value.Equals(analysis.Difficulty, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add("Difficulty must be Beginner, Intermediate, or Advanced.");
        }

        return errors;
    }

    public static string NormalizeDifficulty(string difficulty)
    {
        return AllowedDifficulties.First(value =>
            value.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
    }
}
