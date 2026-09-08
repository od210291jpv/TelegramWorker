using LocalLlmClient;
using Microsoft.AspNetCore.Mvc;

namespace LlmApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LlmController : Controller
    {
        private readonly ILocalLlmService llmService;

        public LlmController(ILocalLlmService llmService)
        {
            this.llmService = llmService;
        }

        [HttpGet("/models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await llmService.GetModelsAsync();
            return Content(models, "application/json");
        }

        [HttpPost("/model")]
        public async Task<IActionResult> SetModel([FromBody] string modelId)
        {
            llmService.SetModel(modelId);
            return Ok();
        }

        [HttpPost("/message")]
        public async Task<string> SendMessage(string message)
        {
            var response = await llmService.SendMessageAsync(message);
            return response;
        }

        [HttpPost("/image-message")]
        public async Task<IActionResult> SendMessageWithImage([FromBody] MessageDto message, string? imageUrl = null)
        {
            var response = await llmService.SendMessageWithImageAsync(message, imageUrl);
            return Content(response, "application/json");
        }
    }
}
