using Moq;
using Track.AI;
using Track.Models;
using Track.Repositories.Interfaces;
using Track.Services;
using Xunit;

namespace Track.Tests
{
    public class PolicyQAServiceTests
    {
        private readonly Mock<IDocumentRepository> _docRepoMock;
        private readonly Mock<IDocumentChunkRepository> _chunkRepoMock;
        private readonly Mock<IQueryLogRepository> _logRepoMock;
        private readonly Mock<IEmbeddingClient> _embeddingMock;
        private readonly Mock<IAIClient> _aiMock;

        private readonly PolicyQAService _service;

        public PolicyQAServiceTests()
        {
            

            _docRepoMock = new Mock<IDocumentRepository>();
            _chunkRepoMock = new Mock<IDocumentChunkRepository>();
            _logRepoMock = new Mock<IQueryLogRepository>();
            _embeddingMock = new Mock<IEmbeddingClient>();
            _aiMock = new Mock<IAIClient>();

            _service = new PolicyQAService(
                _docRepoMock.Object,
                _chunkRepoMock.Object,
                _logRepoMock.Object,
                _embeddingMock.Object,
                _aiMock.Object
            );

        }

        [Fact]
        public async Task AskAsync_Returns_AI_Response()
        {
            // Arrange

            var query = "What is policy coverage?";

            var queryVector = new float[] { 1, 2, 3 };

            _embeddingMock
                .Setup(x => x.GetEmbeddingAsync(query))
                .ReturnsAsync(queryVector);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "policy.txt",
                UploadedAt = DateTime.UtcNow
            };

            _docRepoMock
                .Setup(x => x.GetLatestAsync())
                .ReturnsAsync(document);

            var chunks = new List<DocumentChunk>
            {
                new DocumentChunk
                {
                    Content = "Policy covers accidents",
                    EmbeddingJson = "[1,2,3]"
                },
                new DocumentChunk
                {
                    Content = "Policy excludes fire damage",
                    EmbeddingJson = "[2,3,4]"
                }
            };

            _chunkRepoMock
                .Setup(x => x.GetByDocumentIdAsync(document.Id))
                .ReturnsAsync(chunks);

            _aiMock
                .Setup(x => x.GetCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("This policy covers accidents.");

            // Act

            var result = await _service.AskAsync(query);

            // Assert

            Assert.NotNull(result);

            Assert.Equal(
                "This policy covers accidents.",
                result
            );

            _logRepoMock.Verify(
                x => x.AddAsync(It.IsAny<QueryLog>()),
                Times.Once
            );
        }

        [Fact]
        public async Task AskAsync_Returns_NoDocument_Message()
        {
            // Arrange

            _docRepoMock
                .Setup(x => x.GetLatestAsync())
                .ReturnsAsync((Document?)null);

            // Act

            var result = await _service.AskAsync("test");

            // Assert

            Assert.Equal("No document uploaded.", result);
        }
    }
}