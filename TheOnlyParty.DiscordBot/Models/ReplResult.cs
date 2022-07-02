using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheOnlyParty.DiscordBot.Models;

public class ReplResult
{
    [JsonPropertyName("returnValue")]
    public string? ReturnValue { get; set; }

    [JsonPropertyName("returnTypeName")]
    public string? ReturnTypeName { get; set; }

    [JsonPropertyName("exception")]
    public string? Exception { get; set; }

    [JsonPropertyName("exceptionType")]
    public string? ExceptionType { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("consoleOut")]
    public string? ConsoleOut { get; set; }

    [JsonPropertyName("executionTime")]
    public TimeSpan? ExecutionTime { get; set; }

    [JsonPropertyName("compileTime")]
    public TimeSpan? CompileTime { get; set; }
}