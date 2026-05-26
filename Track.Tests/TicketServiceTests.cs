using Moq;
using Track.AI;
using Track.DTO;
using Track.Models;
using Track.Repositories.Interfaces;
using Track.Services;
using Xunit;

namespace Track.Tests
{
    public class TicketServiceTests
    {
        private readonly Mock<IAIClient> _aiMock;
        private readonly Mock<ITicketRepository> _ticketRepoMock;
        private readonly Mock<IRequestLogRepository> _logRepoMock;

        private readonly TicketService _service;

        public TicketServiceTests()
        {
            _aiMock = new Mock<IAIClient>();
            _ticketRepoMock = new Mock<ITicketRepository>();
            _logRepoMock = new Mock<IRequestLogRepository>();

            _service = new TicketService(
                _aiMock.Object,
                _ticketRepoMock.Object,
                _logRepoMock.Object
            );
        }

        [Fact]
        public async Task CreateTicketAsync_Should_Create_Ticket()
        {
            // Arrange

            var request = new TicketRequest
            {
                CustomerName = "Sindhu",
                Subject = "Login Issue",
                Description = "Unable to login"
            };

            // Act

            var result = await _service.CreateTicketAsync(request);

            // Assert

            Assert.NotNull(result);

            Assert.Equal("Sindhu", result.CustomerName);

            Assert.Equal("Open", result.Status);

            _ticketRepoMock.Verify(
                x => x.AddAsync(It.IsAny<Ticket>()),
                Times.Once);

            _ticketRepoMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTicketsAsync_Should_Return_Tickets()
        {
            // Arrange

            var tickets = new List<Ticket>
            {
                new Ticket { Id = 1, Subject = "Issue1" },
                new Ticket { Id = 2, Subject = "Issue2" }
            };

            _ticketRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(tickets);

            // Act

            var result = await _service.GetAllTicketsAsync();

            // Assert

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SummarizeByIdAsync_Should_Return_Summary()
        {
            // Arrange

            var ticket = new Ticket
            {
                Id = 1,
                CustomerName = "Sindhu",
                Subject = "Login",
                Description = "Cannot login",
                Status = "Open"
            };

            _ticketRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(ticket);

            _aiMock
                .Setup(x => x.GetCompletionAsync(It.IsAny<string>()))
                .ReturnsAsync("Login issue summary");

            // Act

            var result = await _service.SummarizeByIdAsync(1);

            // Assert

            Assert.NotNull(result);

            Assert.Equal(
                "Login issue summary",
                result!.Summary);

            Assert.Equal(
                "Open",
                ticket.Status);

            _logRepoMock.Verify(
                x => x.AddAsync(It.IsAny<RequestLog>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTicketAsync_Should_Return_True()
        {
            // Arrange

            var ticket = new Ticket { Id = 1 };

            _ticketRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(ticket);

            // Act

            var result = await _service.DeleteTicketAsync(1);

            // Assert

            Assert.True(result);

            _ticketRepoMock.Verify(
                x => x.DeleteAsync(ticket),
                Times.Once);
        }

        [Fact]
        public async Task GetTicketByIdAsync_Should_Allow_Customer_Own_Ticket()
        {
            // Arrange

            var ticket = new Ticket
            {
                Id = 1,
                CustomerName = "Sindhu"
            };

            _ticketRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(ticket);

            // Act

            var result = await _service.GetTicketByIdAsync(
                1,
                "Sindhu",
                "Customer");

            // Assert

            Assert.NotNull(result);
        }
    }
}