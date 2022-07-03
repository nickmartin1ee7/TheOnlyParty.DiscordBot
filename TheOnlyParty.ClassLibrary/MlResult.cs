namespace TheOnlyParty.ClassLibrary;

public record MlResult
{
    public bool Positive { get; init; }
    public float Confidence { get; init; }
}
