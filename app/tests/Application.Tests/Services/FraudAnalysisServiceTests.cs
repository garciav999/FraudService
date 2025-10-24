using Application.DTOs;
using Application.Interface;
using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Services;

public class FraudAnalysisServiceTests
{
    private readonly Mock<ILogger<FraudAnalysisService>> _loggerMock;
    private readonly Mock<ITransactionDayRepository> _repositoryMock;
    private readonly FraudAnalysisService _fraudAnalysisService;

    public FraudAnalysisServiceTests()
    {
        _loggerMock = new Mock<ILogger<FraudAnalysisService>>();
        _repositoryMock = new Mock<ITransactionDayRepository>();
        _fraudAnalysisService = new FraudAnalysisService(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenAmountExceedsIndividualLimit_ShouldRejectTransaction()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 3000m);

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Be("Transaction rejected - Individual amount exceeds $2,500 limit");
        result.RiskScore.Should().Be(100m);
        result.RiskFactors.Should().Contain("Individual amount $3,000.00 exceeds $2,500 limit");
        
        // Verify repository was not called for daily total when individual limit exceeded
        _repositoryMock.Verify(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenAmountIsExactlyIndividualLimit_ShouldProcessNormally()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 2500m);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5000m);
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.Reason.Should().Be("Transaction approved");
        result.RiskScore.Should().Be(0m);
        result.RiskFactors.Should().BeEmpty();
        
        _repositoryMock.Verify(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Once);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenDailyTotalExceedsLimit_ShouldRejectTransaction()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 1500m);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(19500m); // Current total that would exceed limit with new transaction

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Be("Transaction rejected - Daily total would exceed $20,500 limit (current: $19,500.00, new total: $21,000.00)");
        result.RiskScore.Should().Be(100m);
        result.RiskFactors.Should().Contain("Daily total $21,000.00 would exceed $20,500 limit");
        
        _repositoryMock.Verify(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Once);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenDailyTotalIsExactlyLimit_ShouldApproveTransaction()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 1000m);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(19500m); // Exactly at limit after adding transaction (20500)
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert - Should be approved because limit is exactly 20500, not exceeding
        result.IsApproved.Should().BeTrue();
        result.Reason.Should().Be("Transaction approved");
        result.RiskScore.Should().Be(0m);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenAllLimitsRespected_ShouldApproveTransaction()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 1500m);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(5000m);
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.Reason.Should().Be("Transaction approved");
        result.RiskScore.Should().Be(0m);
        result.RiskFactors.Should().BeEmpty();
        
        _repositoryMock.Verify(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Once);
        _repositoryMock.Verify(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_WhenRepositoryThrowsException_ShouldUseZeroDailyTotal()
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: 1500m);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database connection failed"));
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().BeTrue(); // Should still approve with zero daily total
        result.Reason.Should().Be("Transaction approved");
        
        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting daily total")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0.01, 0, true)]
    [InlineData(100, 1000, true)]
    [InlineData(2499.99, 18000, true)]
    [InlineData(2500, 18000, true)]
    [InlineData(2500.01, 0, false)] // Exceeds individual limit
    [InlineData(1000, 19500.01, false)] // Exceeds daily limit
    [InlineData(1000, 20500, false)] // Exactly at daily limit
    public async Task AnalyzeTransactionAsync_VariousScenarios_ShouldReturnExpectedResult(
        decimal transactionAmount, 
        decimal currentDailyTotal, 
        bool expectedApproval)
    {
        // Arrange
        var transactionEvent = CreateTransactionEvent(value: transactionAmount);
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(currentDailyTotal);
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        var result = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        result.IsApproved.Should().Be(expectedApproval);
        
        if (expectedApproval)
        {
            result.RiskScore.Should().Be(0m);
            result.RiskFactors.Should().BeEmpty();
            result.Reason.Should().Be("Transaction approved");
        }
        else
        {
            result.RiskScore.Should().Be(100m);
            result.RiskFactors.Should().NotBeEmpty();
            result.Reason.Should().StartWith("Transaction rejected");
        }
    }

    [Fact]
    public async Task AnalyzeTransactionAsync_ShouldUseMidnightUTCForDateComparison()
    {
        // Arrange
        var transactionDate = new DateTime(2025, 10, 24, 15, 30, 45, DateTimeKind.Utc);
        var expectedDate = new DateTime(2025, 10, 24, 0, 0, 0, DateTimeKind.Utc);
        var transactionEvent = CreateTransactionEvent(occurredAt: transactionDate);
        
        _repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), expectedDate))
            .ReturnsAsync(1000m);
        _repositoryMock.Setup(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync("approved");

        // Act
        await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

        // Assert
        _repositoryMock.Verify(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), expectedDate), Times.Once);
    }

    private static TransactionCreatedEvent CreateTransactionEvent(
        decimal value = 1000m,
        DateTime? occurredAt = null,
        Guid? sourceAccountId = null,
        Guid? targetAccountId = null)
    {
        return new TransactionCreatedEvent(
            TransactionExternalId: Guid.NewGuid(),
            SourceAccountId: sourceAccountId ?? Guid.NewGuid(),
            TargetAccountId: targetAccountId ?? Guid.NewGuid(),
            TransferTypeId: 1,
            Value: value,
            Status: "Pending",
            Id: Guid.NewGuid(),
            OccurredAt: occurredAt ?? DateTime.UtcNow,
            EventType: "transaction.created"
        );
    }
}
