using System.Text.Json.Serialization;
using AmazonServices;

namespace Messages;

public class GenerateReportMessage : IMessage
{
    [JsonPropertyName("guid")]
    public Guid Guid { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("requestedUser")]
    public string RequestedUser { get; init; } = default!;

    [JsonIgnore]
    public string MessageTypeName => nameof(GenerateReportMessage);

    public override string ToString()
    {
        return $"A {Name} report was requested by { RequestedUser } with Id: { Guid }";
    }
}