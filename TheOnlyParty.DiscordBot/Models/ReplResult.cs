using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheOnlyParty.DiscordBot.Models;

public record ReplResult
{
    [JsonPropertyName("returnValue")]
    public string? ReturnValue { get; init; }

    [JsonPropertyName("returnTypeName")]
    public string? ReturnTypeName { get; init; }

    [JsonPropertyName("exception")]
    public string? Exception { get; init; }

    [JsonPropertyName("exceptionType")]
    public string? ExceptionType { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("consoleOut")]
    public string? ConsoleOut { get; init; }

    [JsonPropertyName("executionTime")]
    public TimeSpan? ExecutionTime { get; init; }

    [JsonPropertyName("compileTime")]
    public TimeSpan? CompileTime { get; init; }
}