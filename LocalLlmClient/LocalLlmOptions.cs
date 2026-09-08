namespace LocalLlmClient
{
    public class LocalLlmOptions
    {
        public const string SectionName = "LocalLlm";

        public string EndpointUrl { get; set; } = "http://192.168.88.163:11434";

        public string ModelId { get; set; } = "qwen2.5:1.5b";

        public string ApiKey { get; set; } = "no-key";

        public int TimeoutMinutes { get; set; } = 10;

        public string SystemMessage { get; set; } = "";
    }
}
