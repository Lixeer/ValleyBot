using System.Text.Json;

namespace ValleyBot.net.Service;

public class Config<T>
{
    private string EnvPath;
    private string FileName;
    public T Item{get;}
    public Config(string path = null, string filename = null)
    {
        if (path == null) { EnvPath = Path.Combine(AppContext.BaseDirectory); }
        else { EnvPath = path;}
        if (filename == null){ FileName = Path.Combine(EnvPath, "config.json"); }
        else { FileName = Path.Combine(EnvPath, filename); }

        var configText = File.ReadAllText(FileName);
        Item = JsonSerializer.Deserialize<T>(configText);
    }
}