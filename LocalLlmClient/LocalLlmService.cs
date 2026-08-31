using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LocalLlmClient
{
    public partial class LocalLlmService : ILocalLlmService
    {
        private readonly LocalLlmOptions _options;
        private string _currentModelId;
        
        private Kernel _kernel;
        private IChatCompletionService _chatCompletionService;
        private ChatHistory _chatHistory;
        private readonly HttpClient _httpClient;

        public LocalLlmService(LocalLlmOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _currentModelId = _options.ModelId;

            var endpointUrl = _options.EndpointUrl.TrimEnd('/');

            var handler = new OpenAIEndpointHandler(endpointUrl);
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(_options.TimeoutMinutes)
            };

            InitializeKernel();
        }

        private void InitializeKernel()
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: _currentModelId,
                apiKey: _options.ApiKey,
                httpClient: _httpClient);

            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            if (_chatHistory == null)
            {
                _chatHistory = new ChatHistory();
                if (!string.IsNullOrWhiteSpace(_options.SystemMessage))
                {
                    _chatHistory.AddSystemMessage(_options.SystemMessage);
                }
            }
        }

        public void SetModel(string modelId)
        {
            _currentModelId = modelId;
            InitializeKernel();
        }
        
        public void AddTool(object toolInstance, string toolName)
        {
            _kernel.Plugins.AddFromObject(toolInstance, toolName);
        }

        public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            var endpointUrl = _options.EndpointUrl.TrimEnd('/');
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpointUrl}/models");
            
            if (!string.IsNullOrEmpty(_options.ApiKey) && _options.ApiKey != "no-key")
            {
                request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            
            var models = new List<string>();
            if (doc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var idElement))
                    {
                        models.Add(idElement.GetString());
                    }
                }
            }
            
            return models;
        }

        private OpenAIPromptExecutionSettings GetToolExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
        }

        public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            _chatHistory.AddUserMessage(message);

            var response = await _chatCompletionService.GetChatMessageContentAsync(
                _chatHistory, 
                executionSettings: GetToolExecutionSettings(),
                kernel: _kernel, 
                cancellationToken: cancellationToken);

            _chatHistory.AddAssistantMessage(response.Content);

            return response.Content;
        }

        public async Task<string> SendMessageWithImageAsync(string message, string imageUrl, CancellationToken cancellationToken = default)
        {
            var items = new ChatMessageContentItemCollection
            {
                new TextContent(message),
                new ImageContent(new Uri(imageUrl))
            };
            
            _chatHistory.Add(new ChatMessageContent(AuthorRole.User, items));
            
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                _chatHistory, 
                executionSettings: GetToolExecutionSettings(),
                kernel: _kernel, 
                cancellationToken: cancellationToken);
            
            _chatHistory.AddAssistantMessage(response.Content);
            return response.Content;
        }

        public async Task<string> SendMessageWithLocalImageAsync(string message, string localImagePath, CancellationToken cancellationToken = default)
        {
            var imageBytes = await System.IO.File.ReadAllBytesAsync(localImagePath, cancellationToken);
            
            var ext = System.IO.Path.GetExtension(localImagePath)?.ToLowerInvariant();
            var mimeType = ext switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            var items = new ChatMessageContentItemCollection
            {
                new TextContent(message),
                new ImageContent(new ReadOnlyMemory<byte>(imageBytes), mimeType)
            };
            
            _chatHistory.Add(new ChatMessageContent(AuthorRole.User, items));
            
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                _chatHistory, 
                executionSettings: GetToolExecutionSettings(),
                kernel: _kernel, 
                cancellationToken: cancellationToken);
            
            _chatHistory.AddAssistantMessage(response.Content);
            return response.Content;
        }

        public void ClearHistory()
        {
            _chatHistory = new ChatHistory();
            if (!string.IsNullOrWhiteSpace(_options.SystemMessage))
            {
                _chatHistory.AddSystemMessage(_options.SystemMessage);
            }
        }
    }
}
