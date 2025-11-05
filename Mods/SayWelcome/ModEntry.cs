
using ValleyBot;
using ValleyBot.Core.ModManager;

using ValleyBot.Service;

namespace SayWelcome;

public class ModEntry : Mod
{
    private ModConfig config;
       
    private readonly BotApp bot;
    public ModEntry(BotApp bot) : base(bot)
    {
        this.bot = bot;
        config = bot.helper.ReadConfig<ModConfig>();
        Console.WriteLine(config.WelcomeMessage);


    }

    public override void Entry(BotApp bot)
    {
        OnNoticeEvent += SayWelcome_OnNoticeEvent;
        
        
    }



    private async Task SayWelcome_OnNoticeEvent(NoticeEventArgs args)
    {
        var data = args.EventData;
        if (data.NoticeType == "group_increase")
        {
            //Console.WriteLine("群内新成员入群");
            try
            {

                await bot.ActionService.SendJsonAsync(
                new
                {
                    action = "send_group_msg",
                    @params = new
                    {
                        group_id = data.GroupID,
                        message = Builder.BuildTextMessage(config.WelcomeMessage)
                    }
                }

                );


            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送欢迎消息失败: {ex.StackTrace}");

            }

        }
    }
}
