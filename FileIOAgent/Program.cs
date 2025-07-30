using FileIOAgent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

string currentDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "FileAnalyzerWorkSpace"
);
Directory.CreateDirectory(currentDir);
var systemMessage = File.ReadAllText(
    "C:\\Users\\DSU Student\\source\\repos\\FirstChatBot\\FileIOAgent\\system.md"
);
systemMessage = systemMessage.Replace("++dir++", currentDir);

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var builder = Kernel
    .CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "model",
        apiKey: "dummykey",
        endpoint: new Uri("http://127.0.0.1:1234/v1")
    )
    .AddOpenAIEmbeddingGenerator(modelId: "text-embedding-qwen3-embedding-0.6b"); // Use the extension method to add the embedding generator
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Plugins.AddFromType<FileAnalyzerPlugin>();

Kernel kernel = builder.Build();

var embeddingservice = kernel.GetRequiredService<OpenAITextEmbeddingGenerationService>();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var memoryBuilder = new MemoryBuilder();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

memoryBuilder.WithTextEmbeddingGeneration(embeddingservice);
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var memoryService = memoryBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

var conversation = new ChatHistory();
conversation.AddSystemMessage(systemMessage);

while (true)
{
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.Write("Human: ");
    var input = Console.ReadLine()!;
    if (input == "clear")
    {
        conversation = [.. conversation.Take(1)];
        Console.Clear();
    }
    else
    {
        try
        {

            conversation.AddUserMessage(input);
            var results = chatCompletionService.GetStreamingChatMessageContentsAsync(
                conversation,
                executionSettings: openAIPromptExecutionSettings,
                kernel
            );
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("AI: ");
            var aiResponseBuilder = new System.Text.StringBuilder();
            await foreach (var result in results)
            {
                Console.Write(result.Content);
                aiResponseBuilder.Append(result.Content);
            }
            conversation.AddAssistantMessage(
                aiResponseBuilder.ToString().Length > 0
                    ? aiResponseBuilder.ToString()
                    : "No response from AI"
            );
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
