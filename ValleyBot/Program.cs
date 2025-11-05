

using System.Text.Json;
using Onebot.Event;
using ValleyBot.Core.ModManager;

using ValleyBot.Service;

//temp
using System.Collections.Generic;
using System.Diagnostics;

namespace ValleyBot;






public class BotApp
{
    //private List<Mod> mods = new List<Mod>();
    public AdvancedWebSocketClient ActionService;
    Config<BootConfig> _config;
    ModLoader modLoader;
    
    public Helper helper;
    public BotApp()
    {   
        this.helper = new Helper();
        
        this.modLoader = new ModLoader();
        modLoader.LoadMods(this);
        

        _config = new Config<BootConfig>();
        Console.WriteLine(_config.Item.Uri);
        ActionService = new AdvancedWebSocketClient(_config.Item.Uri);
        ActionService.Connected += (sender, e) =>
        {
            Console.WriteLine("✓ WebSocket 已连接");
        };
        ActionService.MessageReceived += Handle1;
        ActionService.MessageReceived += HandleWebSoketMessage;

        ActionService.ErrorOccurred += (sender, ex) =>
        {
            Console.WriteLine($"发生错误: {ex.Message}");
        };
        
        ActionService.Disconnected += (sender, e) =>
        {
            Console.WriteLine("✗ WebSocket 已断开");
        };
    }

    private async void HandleWebSoketMessage(object? sender, string e)
    {
        try
        {
            var preEvent = JsonSerializer.Deserialize<IEvent>(e);
            MetaEvent _metae = null;
            MessageEvent _messagee = null;
            NoticeEvent _noticee = null;
            IEvent fEvent;
            switch (preEvent.PostType)
            {
                case "meta_event":
                    //fEvent = JsonSerializer.Deserialize<MetaEvent>(e);
                    _metae = JsonSerializer.Deserialize<MetaEvent>(e);
                    break;
                case "message":
                    _messagee = JsonSerializer.Deserialize<MessageEvent>(e);
                    break;
                case "notice":
                    _noticee = JsonSerializer.Deserialize<NoticeEvent>(e);
                    break;
                default:
                    fEvent = JsonSerializer.Deserialize<IEvent>(e);
                    break;
            }
            foreach (var mod in modLoader.Mods)
            {

                if (_metae != null)
                {
                    await mod.HandleEvent(_metae);
                }
                else if (_messagee != null)
                {
                    await mod.HandleEvent(_messagee);
                }
                else if (_noticee != null)
                {
                    await mod.HandleEvent(_noticee);
                }
            }



        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序异常: {ex.Message}");
            throw;
        }
        
        //回声事件处理

    }
        

    private async void Handle1(object? sender, string e)
    {

        Console.WriteLine($"收到消息: {e}");

    }

    public async Task RunAsync()

    {
        try
        {
            
            await ActionService.ConnectAsync(TimeSpan.FromSeconds(10));

            Console.WriteLine("\n输入消息并按回车发送，输入 'exit' 退出：");
            
            string input;
            while ((input = Console.ReadLine()) != "exit")
            {

                if (!string.IsNullOrWhiteSpace(input))
                {
                    await ActionService.SendTextAsync(input);
                }
            }
            // 关闭连接
            await ActionService.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序异常: {ex.Message}");
        }
        finally
        {
            ActionService.Dispose();
        }
        Console.WriteLine("程序结束");
    }

    public void t()
    {
        Console.Write("OK");
    }
}


class Program
{

    async static Task Main(string[] args)
    {
        
        BotApp app = new BotApp();
        
        await app.RunAsync();
        Console.WriteLine("finish");    
    }
    // 创建客户端实例



}