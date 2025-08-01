namespace FileIOAgent;

public class FileIndexerService
{
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly RAG _memoryService;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly string _directoryPath;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public FileIndexerService(string directory, RAG memoryService)
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
            for (int i = 0; i < lines.Length; i += 10)
            {
                var chunk = string.Join("\n", lines.Skip(i).Take(10));
                if (string.IsNullOrEmpty(chunk))
                    continue;
                _memoryService.AddDocument(
                    chunk,
                    tag: $"File: {filePath} L#:{i + 1}"
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
