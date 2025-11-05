using System.Reflection;
using System.Text.Json;
using Onebot.Event;

namespace ValleyBot.Core.ModManager

{
    public class ModLoader
    {
        private string localpath;
        private string[] modDirList;
        
        public List<Mod> Mods;

        public ModLoader()
        {
            this.localpath = Path.Combine(AppContext.BaseDirectory, "Mods");
            this.modDirList = Directory.GetDirectories(localpath);
            
            this.Mods = new List<Mod>();

        }
        public void LoadMods(BotApp bot)
        {
            foreach (var modDir in this.modDirList)
            {
                try
                {
                    var modconfig = File.ReadAllText(Path.Combine(modDir, "manifest.json"));
                    var manifest = JsonSerializer.Deserialize<ModMainifest>(modconfig);
                    var dllPath = Path.Combine(modDir, manifest.EntryDll);

                    var assembly = Assembly.LoadFrom(dllPath);
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (typeof(Mod).IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            var modInstance = (Mod)Activator.CreateInstance(type, new object[] { bot })!;
                            Mods.Add(modInstance);

                        }
                    }
                    
                    foreach (var mod in Mods)
                    {
                        Console.WriteLine($"已加载模块: {mod}");
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"加载模块异常: {ex.Message}");
                }
                
            }
        }
    }

    public interface IMod
    {
        public void Entry(BotApp bot);
    }
    
    
    public abstract class Mod:IMod
    {
        public Mod(BotApp bot)
        {
            this.Entry(bot);
        }
        
        public event Func<MessageEventArgs, Task>? OnMessageEvent;
        public event Func<MetaEventArgs, Task>? OnMetaEvent;
        public event Func<NoticeEventArgs, Task>? OnNoticeEvent;
        public abstract void Entry(BotApp bot);

        public async Task HandleEvent(MetaEvent e)
        {   
            //Console.WriteLine("进入处理");
            if (OnMetaEvent == null){return;}
            await OnMetaEvent?.Invoke(new MetaEventArgs{ EventData = e });
        }
        public async Task HandleEvent(MessageEvent e)
        {
            if (OnMessageEvent == null) { return; }
            await OnMessageEvent?.Invoke(new MessageEventArgs { EventData = e });
        }
        public async Task HandleEvent(NoticeEvent e)
        {   
            Console.WriteLine($"收到通知: {e.SubType}");
            if (OnNoticeEvent == null){return;}
            await OnNoticeEvent?.Invoke(new NoticeEventArgs{ EventData = e });
        }
        
    }
    
}