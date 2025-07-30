using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;

namespace FileIOAgent;

public class FileIndexerService
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ISemanticTextMemory _memoryService;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly string _directoryPath;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public FileIndexerService(string directory, ISemanticTextMemory memoryService)
    {
        _directoryPath = directory;
        _memoryService = memoryService;
    }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public async Task IndexAllFilesAsync()
    {
        var files = Directory.GetFiles(_directoryPath, "*", SearchOption.AllDirectories);
        var tasks = files.Select(file => IndexFile(file));
        await Task.WhenAll(tasks);
    }

    private async Task<string> IndexFile(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var lines = content.Split("\n");
            for (int i = 0; i < lines.Length; i+=10)
            {
                var chunk = string.Join("\n", lines.Skip(i).Take(10));
                if (string.IsNullOrEmpty(chunk))
                    continue;
                await _memoryService.SaveInformationAsync(
                    collection: "files",
                    text: chunk,
                    id: $"File: {filePath}L#:{i + 1}",
                    description: $"From {Path.GetFileName(filePath)}"
                );
            }
            return $"Successfully indexed file {Path.GetFileName(filePath)}";
        }
        catch (Exception ex) 
        {
            return $"Error: {ex.Message}";
        }
    }
}
