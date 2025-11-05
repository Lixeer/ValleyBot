using Onebot.Event;
namespace ValleyBot.Core.ModManager
{
    public class MessageEventArgs
    {
        public required MessageEvent EventData{get;set;}
    }

    public class MetaEventArgs
    {
        public required MetaEvent EventData { get; set; }
    }
    public class NoticeEventArgs
    {
        public required NoticeEvent EventData { get; set; }
    }
}