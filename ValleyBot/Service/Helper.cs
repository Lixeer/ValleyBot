using System.Reflection;
using System.Security.Cryptography;

namespace ValleyBot.Service;
public class Helper
{
    public Helper()
    {

    }   

    //读取mod配置文件专用
    public T ReadConfig<T>() where T : new()
    {

        ConfigForMod<T> config = new ConfigForMod<T>(envpath: Path.GetDirectoryName(Assembly.GetCallingAssembly().Location));
        Console.WriteLine($"Helper 被调用了{Assembly.GetCallingAssembly().Location}");
        return config.Item;
    } 
}