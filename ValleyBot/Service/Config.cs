using System.Reflection;
using System.Text.Json;

namespace ValleyBot.Service;

public class Config<T> where T : new()
{
    private string EnvPath;
    private string FileName;

    
    public T Item { get; }
    public Config(string configfilename = "config.json")
    {
        EnvPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        FileName = Path.Combine(EnvPath, configfilename);
        //判断是否存在config文件
        if (!File.Exists(FileName))
        {
            
            File.Create(FileName).Dispose();
            var Json = JsonSerializer.Serialize(new T());
            File.WriteAllText(FileName, Json);
            Console.WriteLine(Json);
        }
        else
        {
            
            var configText = File.ReadAllText(FileName);
            Item = JsonSerializer.Deserialize<T>(configText);
        }



    }
    public void PrintPath()
    {
        Console.WriteLine($"配置文件路径: {FileName}");
    }
}

public class ConfigForMod<T> where T : new()
{
    public T Item;
    public ConfigForMod(string envpath,string configfilename = "config.json")

    {
        var FileName = Path.Combine(envpath, configfilename);
        if (!File.Exists(FileName))
        {
            Console.WriteLine($"配置文件不存在，已创建: {FileName}");
            File.Create(FileName).Dispose();
            var Json = JsonSerializer.Serialize(new T());
            File.WriteAllText(FileName, Json);
            Console.WriteLine(Json);
        }
        else
        {
            Console.WriteLine($"配置文件已存在: {FileName}");
            var configText = File.ReadAllText(FileName);
            Item = JsonSerializer.Deserialize<T>(configText);
        }
    }
}