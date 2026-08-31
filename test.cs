using System; using Microsoft.SemanticKernel; class C { void M() { var b = Kernel.CreateBuilder(); b.AddOpenAIChatCompletion(\
model\, \key\, endpoint: new Uri(\http://local\)); } }
