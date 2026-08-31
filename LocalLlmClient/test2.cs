using System;
using System.IO;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

class C {
    void M() {
        var h = new ChatHistory();
        var items = new ChatMessageContentItemCollection
        {
            new TextContent("hi"),
            new ImageContent(new Uri("http://x.com/a.jpg")),
            new ImageContent(new ReadOnlyMemory<byte>(new byte[0]), "image/png")
        };
        h.Add(new ChatMessageContent(AuthorRole.User, items));
    }
}
