using ValleyBot.Core.ModManager;
using ValleyBot;

namespace PingPong;

public class ModEntry : Mod
{
    private readonly BotApp _app;
    public ModEntry(BotApp bot) : base(bot)
    {
        _app = bot;
    }

    public override void Entry(BotApp bot)
    {

        OnMessageEvent += HandleOPGMessageRecv;
        OnMessageEvent += OnPingRecv;
        Console.WriteLine("hello");
    }

    private async Task HandleOPGMessageRecv(MessageEventArgs args)
    {
        var data = args.EventData;
        string[] githubIndicators = new string[]
        {
            "github.com",
            "http://github.com",
            "https://github.com",
            "https://www.github.com",
            "http://www.github.com"
        };
        //Console.WriteLine("dadaddwa");
        foreach (var indicator in githubIndicators)
        {
            if (!data.RawMessage.Contains(indicator))
            {   
                Console.WriteLine(indicator);
                Console.WriteLine("Github link detected.");
                return;
            }
        }
        
        if (data.MessageType == "group")
        {
            var _sendstring = data.RawMessage;
            foreach (var indicator in githubIndicators)
            {
                _sendstring = _sendstring.Replace(indicator, "http://opengraph.githubassets.com/0");
            }
            await _app.ActionService.SendJsonAsync(
                new
                {
                    action = "send_group_msg",
                    @params = new
                    {
                        group_id = data.GroupID,
                        message = Builder.BuildImgMessage(_sendstring)
                    }
                }
            );
        }
            //var _sendstring = data.RawMessage.Replace("https://github.com", "http://opengraph.githubassets.com/0").Replace("http://github.com", "http://opengraph.githubassets.com/0").Replace("https://www.github.com", "http://opengraph.githubassets.com/0").Replace("github.com", "http://opengraph.githubassets.com/0");
            
            
        }
    
    private async Task OnPingRecv(MessageEventArgs args)
    {   
        Console.WriteLine("ping received");
        var data = args.EventData;

        if (data.MessageType == "group" && data.RawMessage == "ping")
        {
            await _app.ActionService.SendJsonAsync(
                new
                {
                    action = "send_group_msg",
                    @params = new
                    {
                        group_id = data.GroupID,
                        message = Builder.BuildTextMessage("pong")
                    }
                }
            );
        }
    }



}
