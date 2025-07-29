using FirstChatBot;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

string currentDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "FileAnalyzerWorkSpace");
Directory.CreateDirectory(currentDir);
var systemMessage = File.ReadAllText("C:\\Users\\DSU Student\\source\\repos\\FirstChatBot\\FirstChatBot\\system.md");
systemMessage = systemMessage.Replace("++dir++",currentDir);

var builder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
    modelId: "model",
    apiKey: "dummykey",
    endpoint: new Uri("http://127.0.0.1:1234/v1"));

builder.Plugins.AddFromType<FileAnalyzerPlugin>();

Kernel kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
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
            var results = chatCompletionService.
                GetStreamingChatMessageContentsAsync(
                conversation,
                executionSettings: openAIPromptExecutionSettings, 
                kernel);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("AI: ");
            var aiResponseBuilder = new System.Text.StringBuilder();
            await foreach (var result in results)
            {
                Console.Write(result.Content);
                aiResponseBuilder.Append(result.Content);
            }
            conversation.AddAssistantMessage(aiResponseBuilder.ToString().Length > 0 ? aiResponseBuilder.ToString() : "No response from AI");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}