namespace Application.DTOs;

public class UpsertTransactionDayRequest
{
    public DateTime TransactionDate { get; set; }
    public Guid SourceAccountId { get; set; }
    public decimal Value { get; set; }
}