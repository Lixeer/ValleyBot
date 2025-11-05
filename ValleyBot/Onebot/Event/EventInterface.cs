using System.Text.Json.Serialization;

namespace Onebot.Event.Interface
{

    public class BaseEvent
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("self_id")]
        public long SelfId { get; set; }

        [JsonPropertyName("post_type")]
        public string PostType { get; set; }
    }

};