using System.Text.Json.Serialization;

namespace LocalLlmClient
{
    public class MessageDto
    {
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("base64Image")]
        public string? Base64Image { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }
}
