using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileIOAgent;

public class FileSearchPlugin
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ISemanticTextMemory _memoryService;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public FileSearchPlugin(ISemanticTextMemory memoryService)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        _memoryService = memoryService;
    }

    [KernelFunction("search_relevant_files")]
    [Description("Takes in user query and searches for the closest semantic meaning with the stored files")]
    public async Task<string> SearchRelevantFiles(string query)
    {
        var results = _memoryService.SearchAsync("files", query, 3);
        var response = new StringBuilder();
        response.AppendLine("Relevant files found: ");
        await foreach (var result in results)
        {
            response.AppendLine($"Source: {result.Metadata.Id}");
            response.AppendLine(result.Metadata.Text);
            response.AppendLine("-----");
        }
        return response.ToString();
    }
}
