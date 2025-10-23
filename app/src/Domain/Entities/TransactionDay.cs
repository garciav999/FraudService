namespace Domain.Entities;

public class TransactionDay
{
    public Guid TransactionDayId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public decimal TotalValue { get; private set; }
    public DateTime UpdateAt { get; private set; }

    // Constructor for EF Core
    private TransactionDay() { }

    public TransactionDay(DateTime transactionDate, Guid sourceAccountId, decimal totalValue)
    {
        TransactionDate = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);
        SourceAccountId = sourceAccountId;
        TotalValue = totalValue;
        UpdateAt = DateTime.UtcNow;
    }

    public void AddToTotalValue(decimal value)
    {
        TotalValue += value;
        UpdateAt = DateTime.UtcNow;
    }
}
