using Microsoft.EntityFrameworkCore;
using Track.AI;
using Track.Data;
using Track.Helpers;
using Track.Models;

namespace Track.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _db;
        private readonly IEmbeddingClient _embedding;
        private readonly IAIClient _ai;

        public RecommendationService(AppDbContext db, IEmbeddingClient embedding, IAIClient ai)
        {
            _db = db;
            _embedding = embedding;
            _ai = ai;
        }

        public async Task<List<RecommendationResult>> RecommendAsync(List<string> productNames)
        {
            productNames = productNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var products = await _db.Products
                .Where(p => productNames.Contains(p.Name))
                .ToListAsync();

            if (!products.Any())
                return new List<RecommendationResult>();

            var embeddings = await _db.Embeddings.ToListAsync();

            // Collaborative filtering
            var transactions = await _db.Transactions
                .Include(t => t.Items)
                .ToListAsync();

            var pairFreq = new Dictionary<string, int>();

            foreach (var transaction in transactions)
            {
                var items = transaction.Items
                    .Select(i => i.ProductName)
                    .Distinct()
                    .ToList();

                int matchedCount = productNames.Count(p =>
                    items.Any(x => x.Equals(p, StringComparison.OrdinalIgnoreCase)));

                double matchRatio = (double)matchedCount / productNames.Count;

                bool validTransaction;

                if (productNames.Count == 1)
                    validTransaction = matchedCount >= 1;
                else if (productNames.Count <= 3)
                    validTransaction = matchedCount >= 2;
                else
                    validTransaction = matchRatio >= 0.4;

                if (!validTransaction)
                    continue;

                foreach (var item in items)
                {
                    if (productNames.Any(p => p.Equals(item, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    pairFreq[item] = pairFreq.GetValueOrDefault(item) + matchedCount;
                }
            }

            // Hybrid scoring
            var results = new List<RecommendationResult>();

            foreach (var candidate in embeddings)
            {
                if (productNames.Any(p => p.Equals(candidate.ProductName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                double maxSimilarity = 0;

                foreach (var product in products)
                {
                    var queryEmbedding = await _embedding.GetEmbeddingAsync(product.Description);
                    float[] candidateVector = VectorHelper.ParseVector(candidate.Vector);
                    double similarity = VectorHelper.CosineSimilarity(queryEmbedding, candidateVector);

                    if (similarity > maxSimilarity)
                        maxSimilarity = similarity;
                }

                double confidence = pairFreq.ContainsKey(candidate.ProductName)
                    ? pairFreq[candidate.ProductName] : 0;

                double finalScore = confidence < 1
                    ? (0.2 * confidence) + (0.8 * maxSimilarity)
                    : (0.6 * confidence) + (0.4 * maxSimilarity);

                results.Add(new RecommendationResult
                {
                    Product = candidate.ProductName,
                    Confidence = Math.Round(confidence, 2),
                    Similarity = Math.Round(maxSimilarity, 2),
                    FinalScore = Math.Round(finalScore, 2)
                });
            }

            var top = results.OrderByDescending(x => x.FinalScore).Take(3).ToList();

            // AI reasoning
            string cart = string.Join(", ", productNames);

            foreach (var item in top)
            {
                var template = MarkdownLoader.Load("ProductRecommendation.md");

                var prompt = MarkdownLoader.Replace(template, new Dictionary<string, string>
                {
                    { "cart", cart },
                    { "product", item.Product }
                });

                item.Reason = await _ai.GetCompletionAsync(prompt);
            }

            // Log results
            foreach (var item in top)
            {
                _db.RecommendationLogs.Add(new RecommendationLog
                {
                    RequestedProduct = cart,
                    RecommendedProduct = item.Product,
                    Confidence = item.Confidence,
                    Similarity = item.Similarity,
                    FinalScore = item.FinalScore,
                    Reason = item.Reason,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            return top;
        }
    }
}