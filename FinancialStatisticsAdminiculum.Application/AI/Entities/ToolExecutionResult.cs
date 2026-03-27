public class ToolExecutionResult
{
    public bool IsSuccess { get; init; }
    public string Payload { get; init; } = string.Empty;
    public string? Error { get; init; }

    public static ToolExecutionResult Success(string payload)
        => new() { IsSuccess = true, Payload = payload };

    public static ToolExecutionResult Failure(string error)
        => new() { IsSuccess = false, Error = error };
}