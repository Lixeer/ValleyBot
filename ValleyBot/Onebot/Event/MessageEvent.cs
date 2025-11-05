using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Onebot.Event;
// 主事件类，继承基础事件类 IEvent
public class MessageEvent : IEvent
{
    [JsonPropertyName("message_id")]
    public long MessageID { get; set; }

    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; set; }

    [JsonPropertyName("real_id")]
    public long RealID { get; set; }

    [JsonPropertyName("real_seq")]
    public string RealSeq { get; set; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; }

    [JsonPropertyName("raw_message")]
    public string RawMessage { get; set; }

    [JsonPropertyName("font")]
    public int Font { get; set; }

    [JsonPropertyName("sub_type")]
    public string SubType { get; set; }

    [JsonPropertyName("message")]
    public List<MessageContent> Message { get; set; }

    [JsonPropertyName("sender")]
    public Sender Sender { get; set; }

    // 群聊消息专有字段，设置为可空类型
    [JsonPropertyName("group_id")]
    public long? GroupID { get; set; }

    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }

    [JsonPropertyName("message_format")]
    public string MessageFormat { get; set; }
}

// 父类（基础事件类）
public class IEvent
{
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("self_id")]
    public long SelfId { get; set; }

    [JsonPropertyName("post_type")]
    public string PostType { get; set; }
}

// Sender 类：消息发送者信息
public class Sender
{
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("card")]
    public string Card { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }
}

// MessageContent 类：消息的内容
public class MessageContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("data")]
    public MessageData Data { get; set; }
}

// MessageData 类：具体的消息数据
public class MessageData
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
