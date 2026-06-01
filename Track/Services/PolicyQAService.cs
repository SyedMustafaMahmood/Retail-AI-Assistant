using Track.AI;
using Track.Helpers;
using Track.Models;
using Track.Repositories.Interfaces;
using Track.Services;
using System.Text;
using System.Text.Json;

public class PolicyQAService : IPolicyQAService
{
    private readonly IDocumentRepository _docRepo;
    private readonly IDocumentChunkRepository _chunkRepo;
    private readonly IQueryLogRepository _logRepo;
    private readonly IEmbeddingClient _embeddingsClient;
    private readonly IAIClient _geminiClient;

    public PolicyQAService(
        IDocumentRepository docRepo,
        IDocumentChunkRepository chunkRepo,
        IQueryLogRepository logRepo,
        IEmbeddingClient embeddingsClient,
        IAIClient geminiClient)
    {
        _docRepo = docRepo;
        _chunkRepo = chunkRepo;
        _logRepo = logRepo;
        _embeddingsClient = embeddingsClient;
        _geminiClient = geminiClient;
    }

    public async Task UploadDocumentAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var text = await reader.ReadToEndAsync();

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            UploadedAt = DateTime.UtcNow,
            Chunks = new List<DocumentChunk>()
        };

        var chunks = SplitText(text, 500);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingsClient.GetEmbeddingAsync(chunk);

            document.Chunks.Add(new DocumentChunk
            {
                Id = Guid.NewGuid(),
                Content = chunk,
                EmbeddingJson = JsonSerializer.Serialize(embedding)
            });
        }

        await _docRepo.AddAsync(document);
    }

    public async Task<string> AskAsync(string query)
    {
        var queryVector = await _embeddingsClient.GetEmbeddingAsync(query);

        var latestDoc = await _docRepo.GetLatestAsync();
        if (latestDoc == null)
            return "No document uploaded.";

        var chunks = await _chunkRepo.GetByDocumentIdAsync(latestDoc.Id);

        var topChunks = chunks
            .Select(c =>
            {
                var chunkVector = JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)
                                  ?? Array.Empty<float>();

                return new
                {
                    c.Content,
                    Score = chunkVector.Length == 0
                        ? 0
                        : Similarity.Cosine(queryVector, chunkVector)
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Content);

        var contextText = string.Join("\n", topChunks);

        var template = MarkdownLoader.Load("PolicyQA.md");

        var prompt = MarkdownLoader.Replace(template, new Dictionary<string, string>
        {
            { "query", query },
            { "context", contextText }
        });

        var response = await _geminiClient.GetCompletionAsync(prompt);

        await _logRepo.AddAsync(new QueryLog
        {
            Id = Guid.NewGuid(),
            Query = query,
            Response = response,
            Timestamp = DateTime.UtcNow
        });

        return response;
    }

    private List<string> SplitText(string text, int size)
    {
        var list = new List<string>();

        for (int i = 0; i < text.Length; i += size)
        {
            list.Add(text.Substring(i, Math.Min(size, text.Length - i)));
        }

        return list;
    }

    //public async Task AskStreamAsync(string query, HttpResponse response)
    //{
    //    var queryVector = await _embeddingsClient.GetEmbeddingAsync(query);

    //    var latestDoc = await _docRepo.GetLatestAsync();
    //    if (latestDoc == null)
    //    {
    //        await response.WriteAsync("No document uploaded.");
    //        return;
    //    }

    //    var chunks = await _chunkRepo.GetByDocumentIdAsync(latestDoc.Id);

    //    var topChunks = chunks
    //        .Select(c =>
    //        {
    //            var chunkVector = JsonSerializer.Deserialize<float[]>(c.EmbeddingJson)
    //                              ?? Array.Empty<float>();
    //            return new
    //            {
    //                c.Content,
    //                Score = chunkVector.Length == 0
    //                    ? 0
    //                    : Similarity.Cosine(queryVector, chunkVector)
    //            };
    //        })
    //        .OrderByDescending(x => x.Score)
    //        .Take(3)
    //        .Select(x => x.Content);

    //    var contextText = string.Join("\n", topChunks);

    //    var template = MarkdownLoader.Load("PolicyQA.md");

    //    var prompt = MarkdownLoader.Replace(template, new Dictionary<string, string>
    //{
    //    { "query", query },
    //    { "context", contextText }
    //});

    //    // ✅ Stream response word by word
    //    var fullResponse = new StringBuilder();

    //    await foreach (var chunk in _geminiClient.GetCompletionStreamAsync(prompt))
    //    {
    //        await response.WriteAsync(chunk);
    //        await response.Body.FlushAsync();
    //        fullResponse.Append(chunk);
    //    }

    //    // ✅ Log full response
    //    await _logRepo.AddAsync(new QueryLog
    //    {
    //        Id = Guid.NewGuid(),
    //        Query = query,
    //        Response = fullResponse.ToString(),
    //        Timestamp = DateTime.UtcNow
    //    });
    //}
}