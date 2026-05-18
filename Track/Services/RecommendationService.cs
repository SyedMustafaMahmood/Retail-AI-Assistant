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

        public RecommendationService(
            AppDbContext db,
            IEmbeddingClient embedding,
            IAIClient ai)
        {
            _db = db;
            _embedding = embedding;
            _ai = ai;
        }

        public async Task<List<RecommendationResult>> RecommendAsync(
            List<string> productNames)
        {
            productNames = productNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // -----------------------------------------
            // STEP 1: TRY EXACT MATCH FIRST
            // -----------------------------------------
            var normalizedNames = productNames
    .Select(x => x.ToLower())
    .ToList();

            var exactProducts = await _db.Products
                .Where(p => normalizedNames.Contains(p.Name.ToLower()))
                .ToListAsync();

            List<Product> products;

            if (exactProducts.Any())
            {
                products = exactProducts;
            }
            else
            {
                // -----------------------------------------
                // STEP 2: FALLBACK → SEMANTIC MATCHING
                // -----------------------------------------
                products = await ResolveProductsAsync(productNames);
            }

            if (!products.Any())
                return new List<RecommendationResult>();

            var resolvedNames = products
                .Select(p => p.Name)
                .ToList();

            var embeddings = await _db.Embeddings.ToListAsync();

            // -----------------------------------------
            // STEP 3: COLLABORATIVE FILTERING
            // -----------------------------------------
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

                int matchedCount = resolvedNames.Count(p =>
                    items.Any(x =>
                        x.Equals(p, StringComparison.OrdinalIgnoreCase)));

                double matchRatio =
                    (double)matchedCount / resolvedNames.Count;

                bool validTransaction;

                if (resolvedNames.Count == 1)
                    validTransaction = matchedCount >= 1;
                else if (resolvedNames.Count <= 3)
                    validTransaction = matchedCount >= 2;
                else
                    validTransaction = matchRatio >= 0.4;

                if (!validTransaction)
                    continue;

                foreach (var item in items)
                {
                    if (resolvedNames.Any(p =>
                        p.Equals(item, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    pairFreq[item] =
                        pairFreq.GetValueOrDefault(item)
                        + matchedCount;
                }
            }

            // -----------------------------------------
            // STEP 4: CONTENT-BASED FILTERING
            // -----------------------------------------
            var results = new List<RecommendationResult>();

            foreach (var candidate in embeddings)
            {
                if (resolvedNames.Any(p =>
                    p.Equals(candidate.ProductName,
                        StringComparison.OrdinalIgnoreCase)))
                    continue;

                double maxSimilarity = 0;

                float[] candidateVector =
                    VectorHelper.ParseVector(candidate.Vector);

                foreach (var product in products)
                {
                    var queryEmbedding =
                        await _embedding.GetEmbeddingAsync(
                            $"{product.Name} {product.Description}");

                    double similarity =
                        VectorHelper.CosineSimilarity(
                            queryEmbedding,
                            candidateVector);

                    if (similarity > maxSimilarity)
                        maxSimilarity = similarity;
                }

                double confidence =
                    pairFreq.ContainsKey(candidate.ProductName)
                        ? pairFreq[candidate.ProductName]
                        : 0;

                double finalScore =
                    confidence < 1
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

            // -----------------------------------------
            // STEP 5: TOP RESULTS
            // -----------------------------------------
            var top = results
                .OrderByDescending(x => x.FinalScore)
                .Take(3)
                .ToList();

            // -----------------------------------------
            // STEP 6: AI REASONING
            // -----------------------------------------
            string cart = string.Join(", ", resolvedNames);

            foreach (var item in top)
            {
                var template =
                    MarkdownLoader.Load("ProductRecommendation.md");

                var prompt =
                    MarkdownLoader.Replace(
                        template,
                        new Dictionary<string, string>
                        {
                            { "cart", cart },
                            { "product", item.Product }
                        });

                item.Reason =
                    await _ai.GetCompletionAsync(prompt);
            }

            // -----------------------------------------
            // STEP 7: LOGGING
            // -----------------------------------------
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

        // -----------------------------------------
        // SEMANTIC PRODUCT RESOLVER
        // -----------------------------------------
        private async Task<List<Product>> ResolveProductsAsync(
            List<string> inputs)
        {
            var embeddings = await _db.Embeddings.ToListAsync();

            var matchedProducts = new List<Product>();

            foreach (var input in inputs)
            {
                var inputVector =
                    await _embedding.GetEmbeddingAsync(input);

                double bestSimilarity = 0;
                string? bestProduct = null;

                foreach (var candidate in embeddings)
                {
                    float[] candidateVector =
                        VectorHelper.ParseVector(candidate.Vector);

                    double similarity =
                        VectorHelper.CosineSimilarity(
                            inputVector,
                            candidateVector);

                    if (similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestProduct = candidate.ProductName;
                    }
                }

                if (bestSimilarity >= 0.60 &&
                    bestProduct != null)
                {
                    var product =
                        await _db.Products
                            .FirstOrDefaultAsync(p =>
                                p.Name == bestProduct);

                    if (product != null)
                        matchedProducts.Add(product);
                }
            }

            return matchedProducts
                .DistinctBy(p => p.Name)
                .ToList();
        }
    }
}