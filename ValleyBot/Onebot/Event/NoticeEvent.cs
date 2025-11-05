using System.Text.Json.Serialization;
using Onebot.Event.Interface;
namespace Onebot.Event;

public class NoticeEvent : IEvent
{
    [JsonPropertyName("notice_type")]
    public string NoticeType { get; set; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("group_id")]
    public long? GroupID { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserID { get; set; }

    [JsonPropertyName("operator_id")]
    public long? OperatorID { get; set; }
}