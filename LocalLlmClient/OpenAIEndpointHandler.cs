namespace LocalLlmClient
{
    public partial class LocalLlmService
    {
        // A handler to route OpenAI API calls to the local endpoint
        private class OpenAIEndpointHandler : HttpClientHandler
        {
            private readonly string _localEndpoint;

            public OpenAIEndpointHandler(string localEndpoint)
            {
                _localEndpoint = localEndpoint;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.RequestUri != null && request.RequestUri.Host == "api.openai.com")
                {
                    // Redirect api.openai.com to the local endpoint
                    var localUriBuilder = new UriBuilder(_localEndpoint);
                    var originalPath = request.RequestUri.AbsolutePath;
                    
                    // We map the absolute path (e.g. /v1/chat/completions) to our local endpoint
                    // Most local OpenAI endpoints define their base url up to /v1
                    // So we can just append or replace
                    if (originalPath.StartsWith("/v1") && localUriBuilder.Path.EndsWith("/v1"))
                    {
                        // Avoid duplicate /v1
                        localUriBuilder.Path += originalPath.Substring(3);
                    }
                    else
                    {
                        localUriBuilder.Path = localUriBuilder.Path.TrimEnd('/') + originalPath;
                    }
                    
                    localUriBuilder.Query = request.RequestUri.Query;
                    request.RequestUri = localUriBuilder.Uri;
                }

                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
