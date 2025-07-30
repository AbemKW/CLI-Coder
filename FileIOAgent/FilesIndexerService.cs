using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;

namespace FileIOAgent;

public class IndexFiles
{
    //we need directory, embedding service, memroy store
    private readonly ITextEmbeddingGenerationService _embeddingService;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly IMemoryStore _memoryStore;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly string _directoryPath;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public IndexFiles(
        string directory,
        ITextEmbeddingGenerationService embeddingGenerationService,
        IMemoryStore memoryStore
    )
    {
        _directoryPath = directory;
        _embeddingService = embeddingGenerationService;
        _memoryStore = memoryStore;
    }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


}
