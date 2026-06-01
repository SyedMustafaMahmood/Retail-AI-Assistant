using Track.AI;
using Track.Helpers;
using Track.Models;
using Track.Repositories.Interfaces;

namespace Track.Services
{
    public class RecommendationService
    {
        private readonly IProductRepository _productRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IEmbeddingRepository _embeddingRepo;
        private readonly IRecommendationLogRepository _logRepo;
        private readonly IEmbeddingClient _embedding;
        private readonly IAIClient _ai;

        public RecommendationService(
            IProductRepository productRepo,
            ITransactionRepository transactionRepo,
            IEmbeddingRepository embeddingRepo,
            IRecommendationLogRepository logRepo,
            IEmbeddingClient embedding,
            IAIClient ai)
        {
            _productRepo = productRepo;
            _transactionRepo = transactionRepo;
            _embeddingRepo = embeddingRepo;
            _logRepo = logRepo;
            _embedding = embedding;
            _ai = ai;
        }

        public async Task<List<RecommendationResult>> RecommendAsync(List<string> productNames)
        {
            productNames = productNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalized = productNames.Select(x => x.ToLower()).ToList();

            var exactProducts = await _productRepo.GetByNamesAsync(productNames);

            List<Product> products =
                exactProducts.Any()
                    ? exactProducts
                    : await ResolveProductsAsync(productNames);

            if (!products.Any())
                return new List<RecommendationResult>();

            var resolvedNames = products.Select(p => p.Name).ToList();

            var embeddings = await _embeddingRepo.GetAllAsync();
            var transactions = await _transactionRepo.GetAllWithItemsAsync();

            var pairFreq = new Dictionary<string, int>();

            foreach (var transaction in transactions)
            {
                var items = transaction.Items
                    .Select(i => i.ProductName)
                    .Distinct()
                    .ToList();

                int matchedCount = resolvedNames.Count(p =>
                    items.Any(x => x.Equals(p, StringComparison.OrdinalIgnoreCase)));

                // matchRatio logic added back
                double matchRatio = (double)matchedCount / resolvedNames.Count;

                bool validTransaction;

                if (resolvedNames.Count == 1)
                    validTransaction = matchedCount >= 1;
                else if (resolvedNames.Count <= 3)
                    validTransaction = matchedCount >= 2;
                else
                    validTransaction = matchRatio >= 0.4;

                if (!validTransaction) continue;

                foreach (var item in items)
                {
                    if (resolvedNames.Any(p => p.Equals(item, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    pairFreq[item] =
                        pairFreq.GetValueOrDefault(item) + matchedCount;
                }
            }

            var results = new List<RecommendationResult>();

            foreach (var candidate in embeddings)
            {
                if (resolvedNames.Contains(candidate.ProductName,
                    StringComparer.OrdinalIgnoreCase))
                    continue;

                double maxSim = 0;
                float[] candidateVector = VectorHelper.ParseVector(candidate.Vector);

                foreach (var product in products)
                {
                    var queryVec = await _embedding.GetEmbeddingAsync(
                        $"{product.Name} {product.Description}");

                    double sim = VectorHelper.CosineSimilarity(queryVec, candidateVector);

                    if (sim > maxSim)
                        maxSim = sim;
                }

                double confidence =
                    pairFreq.GetValueOrDefault(candidate.ProductName);

                double finalScore =
                    confidence < 1
                        ? (0.2 * confidence) + (0.8 * maxSim)
                        : (0.6 * confidence) + (0.4 * maxSim);

                results.Add(new RecommendationResult
                {
                    Product = candidate.ProductName,
                    Confidence = Math.Round(confidence, 2),
                    Similarity = Math.Round(maxSim, 2),
                    FinalScore = Math.Round(finalScore, 2)
                });
            }

            var top = results.OrderByDescending(x => x.FinalScore).Take(3).ToList();

            string cart = string.Join(", ", resolvedNames);

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

            var logs = top.Select(item => new RecommendationLog
            {
                RequestedProduct = cart,
                RecommendedProduct = item.Product,
                Confidence = item.Confidence,
                Similarity = item.Similarity,
                FinalScore = item.FinalScore,
                Reason = item.Reason,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _logRepo.AddRangeAsync(logs);
            await _logRepo.SaveChangesAsync();

            return top;
        }

        private async Task<List<Product>> ResolveProductsAsync(List<string> inputs)
        {
            var embeddings = await _embeddingRepo.GetAllAsync();
            var result = new List<Product>();

            foreach (var input in inputs)
            {
                var inputVec = await _embedding.GetEmbeddingAsync(input);

                double bestSim = 0;
                string? best = null;

                foreach (var e in embeddings)
                {
                    var vec = VectorHelper.ParseVector(e.Vector);
                    var sim = VectorHelper.CosineSimilarity(inputVec, vec);

                    if (sim > bestSim)
                    {
                        bestSim = sim;
                        best = e.ProductName;
                    }
                }

                if (bestSim >= 0.80 && best != null)
                {
                    var product = await _productRepo.GetByNameAsync(best);
                    if (product != null)
                        result.Add(product);
                }
            }

            return result.DistinctBy(p => p.Name).ToList();
        }
    }
}