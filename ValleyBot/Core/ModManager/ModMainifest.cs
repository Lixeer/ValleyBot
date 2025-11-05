using System.Text.Json.Serialization;

namespace ValleyBot.Core.ModManager
{
    public class ModMainifest
    {   
        [JsonPropertyName("mod_name")]
        public string ModName { get; set; }
        [JsonPropertyName("version")]
        public string ModVersion { get; set; }
        [JsonPropertyName("author")]
        public string ModAuthor { get; set; }
        [JsonPropertyName("description")]
        public string ModDescription { get; set; }
        [JsonPropertyName("entry_dll")]
        public string EntryDll { get; set; }  
    }
}