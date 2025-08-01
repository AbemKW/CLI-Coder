using SentenceTransformers.MiniLM;
using ReadSharp;
using System.Net;
using SentenceTransformers;
namespace FileIOAgent;
public class RAG(int chunkSize = 100, int overlap = 0)
{
    SentenceEncoder Encoder = new SentenceEncoder();
    List<(string text,string tag)> Documents = [];
    TaggedEncodedChunk[] Encoded = [];
    int chunkSize = chunkSize;
    int overlap = overlap;

    public void AddWebsites(params string[] Websites)
    {
        async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }
        foreach (var fullUrl in Websites)
        {
            var response = CallUrl(fullUrl).Result;

            AddDocument(HtmlUtilities.ConvertToPlainText(response), tag: $"Website: {fullUrl}");
        }
    }

    public void AddDocument(string document, string tag = "default")
    {
        Documents.Add((document, tag));
        Compile();
    }

    public void Compile()
    {
        // If no documents added, clear the search index and stop
        if (Documents.Count == 0)
        {
            Encoded = [];
            return;
        }

        // List to hold all small text pieces with their source labels
        var allTextPieces = new List<(string Text, string Tag)>();

        int stepForwardAmount = chunkSize - overlap;

        // Process each document one by one
        foreach (var (documentText, sourceTag) in Documents)
        {
            // Break the document into individual words/tokens
            var wordTokens = Encoder.Tokenizer.TokenizeSimple(documentText);

            // Create chunks by sliding through the words
            for (int startIndex = 0; startIndex < wordTokens.Count; startIndex += stepForwardAmount)
            {
                // How many words to take in this chunk
                int wordsToTake = Math.Min(chunkSize, wordTokens.Count - startIndex);

                // Get the actual words for this chunk
                var wordsInThisChunk = wordTokens.Skip(startIndex).Take(wordsToTake).ToArray();

                // Join them back into a readable sentence/paragraph
                // ❗ Important: Use space " " not ""!
                string chunkText = string.Join(" ", wordsInThisChunk);

                // Only keep the chunk if it has real content
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    // Save the text and where it came from
                    allTextPieces.Add((Text: chunkText, Tag: sourceTag));
                }
            }
        }

        // Now turn all text pieces into numerical meaning-vectors (embeddings)
        var textsToEncode = allTextPieces.Select(piece => piece.Text).ToArray();
        var meaningVectors = Encoder.Encode(textsToEncode);

        // Build the final searchable database
        Encoded = new TaggedEncodedChunk[allTextPieces.Count];

        for (int i = 0; i < allTextPieces.Count; i++)
        {
            Encoded[i] = new TaggedEncodedChunk
            {
                Text = allTextPieces[i].Text,
                Vector = meaningVectors[i],
                Tag = allTextPieces[i].Tag
            };
        }
    }
    public record RagResult(string Text, string Tag);
    public RagResult[] FindNearestK(string queryText, long topk = 3)
    {
        var query = Encoder.Encode([queryText])[0];
        var enc = Encoded.ToList();
        topk = Math.Min(enc.Count, topk);
        List<RagResult> results = [];
        for (int i = 0; i < topk; i++)
        {
            int idx = FindNearest(query, [.. enc]);
            results.Add(new (enc[idx].Text, enc[idx].Tag));
            enc.RemoveAt(idx);
        }
        return [.. results];
    }
    public int FindNearest(float[] query, TaggedEncodedChunk[] chunks)
    {
        float maxDotProduct = 0;
        int bestIndex = 0;
        var normQuery = NormalizeVector(query);
        for (int i = 0; i < chunks.Length; i++)
        {
            float dotProd = DotProduct(NormalizeVector(chunks[i].Vector), normQuery);
            if (dotProd > maxDotProduct)
            {
                maxDotProduct = dotProd;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
    static float Length(float[] vector)
    {
        float sum = 0;

        for (int i = 0; i < vector.Length; i++)
        {
            sum += vector[i] * vector[i];
        }
        return (float)Math.Sqrt(sum);
    }
    public static float[] NormalizeVector(float[] vector)
    {
        var length = Length(vector);
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = vector[i] / length;
        }
        return vector;
    }
    public static float DotProduct(float[] a, float[] b)
    {
        float sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }
}