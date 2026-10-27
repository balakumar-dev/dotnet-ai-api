using System.Text.Json;
using DotNetAI.Core.DTOs;

namespace DotNetAI.Core.Validation;

public static class TopicAnalysisParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static TopicAnalysis Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new InvalidOperationException("The AI returned an empty response.");
        }

        string json = ExtractJson(rawText);

        TopicAnalysis? analysis;
        try
        {
            analysis = JsonSerializer.Deserialize<TopicAnalysis>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("The AI returned invalid JSON.", ex);
        }

        IReadOnlyList<string> errors = TopicAnalysisValidator.Validate(analysis);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "The AI returned a response that failed validation: " + string.Join(" ", errors));
        }

        analysis!.Difficulty = TopicAnalysisValidator.NormalizeDifficulty(analysis.Difficulty);
        analysis.Technologies = analysis.Technologies
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();
        analysis.Summary = analysis.Summary.Trim();

        return analysis;
    }

    private static string ExtractJson(string rawText)
    {
        string trimmed = rawText.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        int firstNewLine = trimmed.IndexOf('\n');
        int lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);

        if (firstNewLine < 0 || lastFence <= firstNewLine)
        {
            return trimmed;
        }

        return trimmed[(firstNewLine + 1)..lastFence].Trim();
    }
}
