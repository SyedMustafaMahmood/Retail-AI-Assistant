using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Track.AI;
using Track.Data;
using Track.Helpers;
using Track.Models;

namespace Track.Services
{
    public class PolicyQAService : IPolicyQAService
    {
        private readonly AppDbContext _context;
        private readonly IEmbeddingClient _embeddingsClient;
        private readonly IAIClient _geminiClient;

        public PolicyQAService(
            AppDbContext context,
            IEmbeddingClient embeddingsClient,
            IAIClient geminiClient)
        {
            _context = context;
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

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task<string> AskAsync(string query)
        {
            // 1. Get query embedding
            var queryVector = await _embeddingsClient.GetEmbeddingAsync(query);

            // 2. Get latest document
            var latestDoc = await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync();

            if (latestDoc == null)
                return "No document uploaded.";

            // 3. Get chunks
            var chunks = await _context.DocumentChunks
                .Where(c => c.DocumentId == latestDoc.Id)
                .ToListAsync();

            // 4. Compute similarity
            var topChunks = chunks
                .Select(c =>
                {
                    var chunkVector = JsonSerializer.Deserialize<float[]>(c.EmbeddingJson) ?? Array.Empty<float>();

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

            // 5. Build context
            var contextText = string.Join("\n", topChunks);

            // 6. Prompt
            var template = MarkdownLoader.Load("PolicyQA.md");

            var prompt = MarkdownLoader.Replace(template, new Dictionary<string, string>
            {
                { "query", query },
                { "context", contextText }
            });

            // 7. Call AI
            var response = await _geminiClient.GetCompletionAsync(prompt);

            // 8. Log result
            _context.QueryLogs.Add(new QueryLog
            {
                Id = Guid.NewGuid(),
                Query = query,
                Response = response,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

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
    }
}