using System.Text.Json.Serialization;

namespace ValleyBot;

public class BootConfig
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}