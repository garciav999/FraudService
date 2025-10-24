using System.Text.Json.Serialization;

namespace Application.DTOs;

public record TransactionCreatedEvent(
    [property: JsonPropertyName("transactionExternalId")] Guid TransactionExternalId,
    [property: JsonPropertyName("sourceAccountId")] Guid SourceAccountId,
    [property: JsonPropertyName("targetAccountId")] Guid TargetAccountId,
    [property: JsonPropertyName("transferTypeId")] int TransferTypeId,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("occurredAt")] DateTime OccurredAt,
    [property: JsonPropertyName("eventType")] string EventType
);

public record TransactionStatusEvent(
    Guid TransactionId,
    string Status, // "approved" or "rejected"
    string Reason,
    DateTime ProcessedAt,
    string ProcessedBy = "anti-fraud-service"
);