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
        if (Documents.Count == 0)
        {
            Encoded = [];
            return;
        }

        var chunks = new List<(string Text, string Tag)>();
        int stride = chunkSize - overlap;

        // Create chunks for each document
        foreach (var (docText, docTag) in Documents)
        {
            // Split document into tokens
            var tokens = Encoder.Tokenizer.TokenizeSimple(docText);

            // Create chunks
            for (int i = 0; i < tokens.Count; i += stride)
            {
                // Take chunkSize tokens or remaining tokens if less than chunkSize
                int currentChunkSize = Math.Min(chunkSize, tokens.Count - i);
                var chunkWords = tokens.Skip(i).Take(currentChunkSize).ToArray();
                string chunkText = string.Join(" ", chunkWords);

                // Only add non-empty chunks
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    chunks.Add((chunkText, docTag));
                }
            }
        }

        // Encode chunks
        var chunkTexts = chunks.Select(c => c.Text).ToArray();
        var encodedVectors = Encoder.Encode(chunkTexts);

        // Create TaggedEncodedChunk array
        Encoded = new TaggedEncodedChunk[chunks.Count];
        for (int i = 0; i < chunks.Count; i++)
        {
            Encoded[i] = new TaggedEncodedChunk
            {
                Text = chunks[i].Text,
                Vector = encodedVectors[i],
                Tag = chunks[i].Tag
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