namespace LocalLlmClient
{
    public class LocalLlmOptions
    {
        public const string SectionName = "LocalLlm";

        public string EndpointUrl { get; set; } = "http://127.0.0.1:1234/v1";
        public string ModelId { get; set; } = "local-model";
        public string ApiKey { get; set; } = "no-key";
        public int TimeoutMinutes { get; set; } = 10;
        public string SystemMessage { get; set; } = "You are a helpful AI assistant.";
    }
}
