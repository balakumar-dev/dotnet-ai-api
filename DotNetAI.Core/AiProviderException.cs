namespace DotNetAI.Core;

public sealed class AiProviderException : InvalidOperationException
{
    public int StatusCode { get; }

    public AiProviderException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
