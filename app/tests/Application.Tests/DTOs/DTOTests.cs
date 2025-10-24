using Application.DTOs;
using Application.Interface;
using FluentAssertions;

namespace Application.Tests.DTOs;

public class TransactionCreatedEventTests
{
    [Fact]
    public void TransactionCreatedEvent_ShouldBeInitializedCorrectly()
    {
        // Arrange
        var transactionExternalId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        var transferTypeId = 1;
        var value = 1500.50m;
        var status = "Pending";
        var id = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var eventType = "transaction.created";

        // Act
        var transactionEvent = new TransactionCreatedEvent(
            transactionExternalId,
            sourceAccountId,
            targetAccountId,
            transferTypeId,
            value,
            status,
            id,
            occurredAt,
            eventType
        );

        // Assert
        transactionEvent.TransactionExternalId.Should().Be(transactionExternalId);
        transactionEvent.SourceAccountId.Should().Be(sourceAccountId);
        transactionEvent.TargetAccountId.Should().Be(targetAccountId);
        transactionEvent.TransferTypeId.Should().Be(transferTypeId);
        transactionEvent.Value.Should().Be(value);
        transactionEvent.Status.Should().Be(status);
        transactionEvent.Id.Should().Be(id);
        transactionEvent.OccurredAt.Should().Be(occurredAt);
        transactionEvent.EventType.Should().Be(eventType);
    }

    [Fact]
    public void TransactionCreatedEvent_WithDefaultValues_ShouldWork()
    {
        // Arrange & Act
        var transactionEvent = new TransactionCreatedEvent(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            0,
            0m,
            string.Empty,
            Guid.Empty,
            DateTime.MinValue,
            string.Empty
        );

        // Assert
        transactionEvent.TransactionExternalId.Should().Be(Guid.Empty);
        transactionEvent.SourceAccountId.Should().Be(Guid.Empty);
        transactionEvent.TargetAccountId.Should().Be(Guid.Empty);
        transactionEvent.TransferTypeId.Should().Be(0);
        transactionEvent.Value.Should().Be(0m);
        transactionEvent.Status.Should().Be(string.Empty);
        transactionEvent.Id.Should().Be(Guid.Empty);
        transactionEvent.OccurredAt.Should().Be(DateTime.MinValue);
        transactionEvent.EventType.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1500.50)]
    [InlineData(2500.00)]
    [InlineData(99999.99)]
    public void TransactionCreatedEvent_WithVariousValues_ShouldStoreCorrectly(decimal value)
    {
        // Arrange & Act
        var transactionEvent = new TransactionCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            value,
            "Pending",
            Guid.NewGuid(),
            DateTime.UtcNow,
            "transaction.created"
        );

        // Assert
        transactionEvent.Value.Should().Be(value);
    }
}

public class TransactionStatusEventTests
{
    [Fact]
    public void TransactionStatusEvent_ShouldBeInitializedCorrectly()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var status = "Approved";
        var reason = "Transaction approved";
        var processedAt = DateTime.UtcNow;

        // Act
        var statusEvent = new TransactionStatusEvent(
            transactionId,
            status,
            reason,
            processedAt
        );

        // Assert
        statusEvent.TransactionId.Should().Be(transactionId);
        statusEvent.Status.Should().Be(status);
        statusEvent.Reason.Should().Be(reason);
        statusEvent.ProcessedAt.Should().Be(processedAt);
    }

    [Theory]
    [InlineData("Approved", "Transaction approved")]
    [InlineData("Rejected", "Individual amount exceeds limit")]
    [InlineData("Failed", "System error occurred")]
    public void TransactionStatusEvent_WithVariousStatuses_ShouldStoreCorrectly(string status, string reason)
    {
        // Arrange & Act
        var statusEvent = new TransactionStatusEvent(
            Guid.NewGuid(),
            status,
            reason,
            DateTime.UtcNow
        );

        // Assert
        statusEvent.Status.Should().Be(status);
        statusEvent.Reason.Should().Be(reason);
    }
}

public class FraudAnalysisResultTests
{
    [Fact]
    public void FraudAnalysisResult_ShouldBeInitializedCorrectly()
    {
        // Arrange
        var isApproved = true;
        var reason = "Transaction approved";
        var riskScore = 0m;
        var riskFactors = new List<string>();

        // Act
        var result = new FraudAnalysisResult(isApproved, reason, riskScore, riskFactors);

        // Assert
        result.IsApproved.Should().Be(isApproved);
        result.Reason.Should().Be(reason);
        result.RiskScore.Should().Be(riskScore);
        result.RiskFactors.Should().BeEquivalentTo(riskFactors);
    }

    [Fact]
    public void FraudAnalysisResult_WithRiskFactors_ShouldStoreCorrectly()
    {
        // Arrange
        var riskFactors = new List<string>
        {
            "Individual amount exceeds limit",
            "Daily total exceeds limit"
        };

        // Act
        var result = new FraudAnalysisResult(false, "Rejected", 100m, riskFactors);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.Reason.Should().Be("Rejected");
        result.RiskScore.Should().Be(100m);
        result.RiskFactors.Should().HaveCount(2);
        result.RiskFactors.Should().Contain("Individual amount exceeds limit");
        result.RiskFactors.Should().Contain("Daily total exceeds limit");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void FraudAnalysisResult_WithVariousRiskScores_ShouldStoreCorrectly(decimal riskScore)
    {
        // Arrange & Act
        var result = new FraudAnalysisResult(true, "Test", riskScore, new List<string>());

        // Assert
        result.RiskScore.Should().Be(riskScore);
    }
}

public class UpsertTransactionDayRequestTests
{
    [Fact]
    public void UpsertTransactionDayRequest_ShouldBeInitializedCorrectly()
    {
        // Arrange
        var transactionDate = DateTime.UtcNow;
        var sourceAccountId = Guid.NewGuid();
        var value = 1500.50m;

        // Act
        var request = new UpsertTransactionDayRequest
        {
            TransactionDate = transactionDate,
            SourceAccountId = sourceAccountId,
            Value = value
        };

        // Assert
        request.TransactionDate.Should().Be(transactionDate);
        request.SourceAccountId.Should().Be(sourceAccountId);
        request.Value.Should().Be(value);
    }

    [Fact]
    public void UpsertTransactionDayRequest_WithDefaultConstructor_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var request = new UpsertTransactionDayRequest();

        // Assert
        request.TransactionDate.Should().Be(default(DateTime));
        request.SourceAccountId.Should().Be(default(Guid));
        request.Value.Should().Be(default(decimal));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1500.50)]
    [InlineData(2500.00)]
    [InlineData(-100.00)] // Testing negative values
    public void UpsertTransactionDayRequest_WithVariousValues_ShouldStoreCorrectly(decimal value)
    {
        // Arrange & Act
        var request = new UpsertTransactionDayRequest
        {
            TransactionDate = DateTime.UtcNow,
            SourceAccountId = Guid.NewGuid(),
            Value = value
        };

        // Assert
        request.Value.Should().Be(value);
    }
}