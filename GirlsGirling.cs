using System.Text.Json.Serialization;

namespace losertron4000
{
    public class GirlsGirling
    {
        [JsonPropertyName("groups")]
        public string[][] Groups { get; set; }

        [JsonPropertyName("folders")]
        public GirlieFolder[] Folders { get; set; }
       
    }

    public class GirlieFolder
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bypass")]
        public string[] Bypass { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("default")]
        public string[] Default { get; set; }

        [JsonPropertyName("z-index")]
        public int ZIndex { get; set; }

    }
}