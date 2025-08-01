using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace FileIOAgent;

public class FileSearchPlugin
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly RAG _memoryService;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public FileSearchPlugin(RAG memoryService)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        _memoryService = memoryService;
    }

    [KernelFunction("search_relevant_files")]
    [Description("Takes in user query and searches for the closest semantic meaning with the stored files, returns the source and the matched content")]
    public async Task<string> SearchRelevantFiles(string query, int topK)
    {
        // Use SearchAsync with correct parameters and await the result
        var searchResult = _memoryService.FindNearestK(query, topK);
        var response = new StringBuilder();
        response.AppendLine("Relevant files found: ");
        if (searchResult.Count() > 0)
        {
            foreach (var result in searchResult)
            {
                response.AppendLine($"Source: {result.Tag}");
                response.AppendLine(result.Text);
                response.AppendLine("-----");
            }
        }
        else
        {
            response.AppendLine("No relevant files found.");
        }
        return response.ToString();
    }
}
