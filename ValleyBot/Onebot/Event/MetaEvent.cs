

using System.Text.Json.Serialization;
using Onebot.Event.Interface;
namespace Onebot.Event;

public class MetaEvent : IEvent
{


    [JsonPropertyName("meta_event_type")]
    public string MetaEventType { get; set; }

    
    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }
    
    
    [JsonPropertyName("status")]
    public Status? Status { get; set; }

    
    
    [JsonPropertyName("interval")]
    public int? Interval{ get; set; }



}

public class Status
{
    [JsonPropertyName("online")]
    public bool Online { get; set; }

    [JsonPropertyName("good")]
    public bool Good { get; set; }

    
}