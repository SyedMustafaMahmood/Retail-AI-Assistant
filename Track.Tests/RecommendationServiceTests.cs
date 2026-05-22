using Moq;
using Track.AI;
using Track.Models;
using Track.Repositories.Interfaces;
using Track.Services;
using Xunit;

namespace Track.Tests
{
    public class RecommendationServiceTests
    {
        private readonly Mock<IProductRepository> _productRepo = new();
        private readonly Mock<ITransactionRepository> _transactionRepo = new();
        private readonly Mock<IEmbeddingRepository> _embeddingRepo = new();
        private readonly Mock<IRecommendationLogRepository> _logRepo = new();
        private readonly Mock<IEmbeddingClient> _embedding = new();
        private readonly Mock<IAIClient> _ai = new();

        private readonly RecommendationService _service;

        public RecommendationServiceTests()
        {
            _service = new RecommendationService(
                _productRepo.Object,
                _transactionRepo.Object,
                _embeddingRepo.Object,
                _logRepo.Object,
                _embedding.Object,
                _ai.Object
            );
        }

        [Fact]
        public async Task RecommendAsync_Returns_Empty_When_No_Products()
        {
            // Mock exact match returning empty
            _productRepo
                .Setup(x => x.GetByNamesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Product>());

            // ✅ Add this — mock embeddings so resolver doesn't crash
            _embeddingRepo
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<EmbeddingMetadata>());

            var result = await _service.RecommendAsync(new List<string> { "abc" });

            Assert.Empty(result);
        }

        [Fact]
        public async Task RecommendAsync_Returns_Results_When_Data_Exists()
        {
            _productRepo
                .Setup(x => x.GetByNamesAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new List<Product>
                {
                    new Product { Name = "Phone", Description = "Smart phone" }
                });

            _embeddingRepo
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<EmbeddingMetadata>
                {
                    new EmbeddingMetadata { ProductName = "Headphones", Vector = "[1,2,3]" }
                });

            _transactionRepo
                .Setup(x => x.GetAllWithItemsAsync())
                .ReturnsAsync(new List<Transaction>());

            _embedding
                .Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[] { 1, 2, 3 });

            _ai
                .Setup(x => x.GetCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Good match");

            var result = await _service.RecommendAsync(new List<string> { "Phone" });

            Assert.NotNull(result);
        }
    }
}