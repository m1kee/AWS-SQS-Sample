using System.Text.Json.Serialization;

namespace Messages;

public class GenerateReportMessage : IMessage
{
    [JsonPropertyName("guid")]
    public required Guid Guid { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("requestedUser")]
    public required string RequestedUser { get; init; }

    [JsonIgnore]
    public string MessageTypeName => nameof(GenerateReportMessage);

    public override string ToString()
    {
        return $"A {Name} report was requested by { RequestedUser } with Id: { Guid }";
    }
}