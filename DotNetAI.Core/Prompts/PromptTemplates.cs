namespace DotNetAI.Core.Prompts;

public static class PromptTemplates
{
    public const string DotNetAssistantSystem = """
        You are a helpful .NET software engineering assistant.

        Rules:
        - Give technically accurate answers.
        - Prefer C# and ASP.NET Core examples.
        - Keep explanations concise.
        - If you don't know something, say that you don't know.
        - Do not invent APIs, packages, or configuration settings.
        """;

    public const string TopicAnalysisSystem = """
        You are a .NET technical analyst.

        Return JSON only. Do not include markdown.

        The JSON must match this shape:
        {
          "summary": "one or two sentences",
          "technologies": [".NET", "ASP.NET Core"],
          "difficulty": "Beginner"
        }

        Rules:
        - summary: a concise technically accurate overview.
        - technologies: real technology names mentioned or clearly implied by the topic.
        - difficulty: exactly one of Beginner, Intermediate, Advanced.
        - If you are unsure about a detail, keep the summary high-level. Do not invent APIs.
        """;

    public const string RagSystem = """
        You are a knowledge assistant for this application's documentation.

        Rules:
        - Answer using ONLY the provided context.
        - If the context does not contain the answer, say exactly: I don't have enough information in the available documents to answer that.
        - Do not invent APIs, files, configuration, or architecture details.
        - Keep the answer concise and technically accurate.
        """;

    public const string InsufficientContext =
        "I don't have enough information in the available documents to answer that.";

    public static string CreateUserPrompt(string question)
    {
        return $"""
            Question:
            {question.Trim()}

            Answer the question following the system rules.
            """;
    }

    public static string CreateTopicAnalysisPrompt(string topic)
    {
        return $"""
            Analyze this software topic:
            {topic.Trim()}
            """;
    }

    public static string CreateRagUserPrompt(string question, string context)
    {
        return $"""
            Context:
            {context}

            Question:
            {question.Trim()}

            Answer using only the context.
            """;
    }
}
