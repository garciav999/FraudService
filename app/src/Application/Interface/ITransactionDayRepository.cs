using Domain.Entities;

namespace Application.Interface;

public interface ITransactionDayRepository
{
    Task InsertAsync(TransactionDay transactionDay);
    Task<string> UpsertTransactionDayAsync(DateTime transactionDate, Guid sourceAccountId, decimal value);
    Task<decimal> GetDailyTotalAsync(Guid sourceAccountId, DateTime targetDate);
}