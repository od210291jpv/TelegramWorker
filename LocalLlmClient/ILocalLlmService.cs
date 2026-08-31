using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalLlmClient
{
    public interface ILocalLlmService
    {
        Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default);
        Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
        Task<string> SendMessageWithImageAsync(string message, string imageUrl, CancellationToken cancellationToken = default);
        Task<string> SendMessageWithLocalImageAsync(string message, string localImagePath, CancellationToken cancellationToken = default);
        void ClearHistory();
        void SetModel(string modelId);
        
        /// <summary>
        /// Registers a native SK tool (plugin) to the underlying Kernel.
        /// </summary>
        void AddTool(object toolInstance, string toolName);
    }
}
