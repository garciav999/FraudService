using Application.Commands;
using Application.Interface;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands;

public class TransactionDayCommandsTests
{
    private readonly Mock<ITransactionDayRepository> _repositoryMock;
    private readonly TransactionDayCommands _commands;

    public TransactionDayCommandsTests()
    {
        _repositoryMock = new Mock<ITransactionDayRepository>();
        _commands = new TransactionDayCommands(_repositoryMock.Object);
    }

    [Fact]
    public async Task UpsertTransactionDayAsync_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 10, 24, 14, 30, 0, DateTimeKind.Utc);
        var sourceAccountId = Guid.NewGuid();
        var value = 1500.50m;
        var expectedResult = "approved";

        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commands.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value);

        // Assert
        result.Should().Be(expectedResult);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value), Times.Once);
    }

    [Fact]
    public async Task UpsertTransactionDayAsync_WithZeroAmount_ShouldStillCallRepository()
    {
        // Arrange
        var transactionDate = DateTime.UtcNow;
        var sourceAccountId = Guid.NewGuid();
        var value = 0m;
        var expectedResult = "approved";

        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commands.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value);

        // Assert
        result.Should().Be(expectedResult);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value), Times.Once);
    }

    [Fact]
    public async Task UpsertTransactionDayAsync_WithNegativeAmount_ShouldStillCallRepository()
    {
        // Arrange
        var transactionDate = DateTime.UtcNow;
        var sourceAccountId = Guid.NewGuid();
        var value = -100m;
        var expectedResult = "approved";

        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commands.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value);

        // Assert
        result.Should().Be(expectedResult);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value), Times.Once);
    }

    [Theory]
    [InlineData(1.50)]
    [InlineData(2500.00)]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    public async Task UpsertTransactionDayAsync_WithVariousAmounts_ShouldPassThrough(decimal amount)
    {
        // Arrange
        var transactionDate = DateTime.UtcNow;
        var sourceAccountId = Guid.NewGuid();
        var expectedResult = "approved";

        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, amount))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _commands.UpsertTransactionDayAsync(transactionDate, sourceAccountId, amount);

        // Assert
        result.Should().Be(expectedResult);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, amount), Times.Once);
    }

    [Fact]
    public async Task UpsertTransactionDayAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var transactionDate = DateTime.UtcNow;
        var sourceAccountId = Guid.NewGuid();
        var value = 1500m;
        var expectedException = new InvalidOperationException("Database error");

        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _commands.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value));
        
        exception.Should().Be(expectedException);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(transactionDate, sourceAccountId, value), Times.Once);
    }
}