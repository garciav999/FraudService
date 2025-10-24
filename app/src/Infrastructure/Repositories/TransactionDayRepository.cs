using Application.Interface;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class TransactionDayRepository : ITransactionDayRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionDayRepository> _logger;

    public TransactionDayRepository(ApplicationDbContext context, ILogger<TransactionDayRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InsertAsync(TransactionDay transactionDay)
    {
        await _context.TransactionDays.AddAsync(transactionDay);
        await _context.SaveChangesAsync();
    }

    public async Task<string> UpsertTransactionDayAsync(DateTime transactionDate, Guid sourceAccountId, decimal value)
    {
        // Normaliza la fecha a UTC y sin hora
        var targetDate = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);

        var existingTransaction = await _context.TransactionDays
            .Where(td => td.SourceAccountId == sourceAccountId)
            .Where(td => td.TransactionDate.Date == targetDate.Date)
            .FirstOrDefaultAsync();

        if (existingTransaction != null)
        {
            existingTransaction.AddToTotalValue(value);
        }
        else
        {
            var newTransactionDay = new TransactionDay(targetDate, sourceAccountId, value);
            await _context.TransactionDays.AddAsync(newTransactionDay);
        }

        await _context.SaveChangesAsync();
        // Retorna "approved" porque la validación ya fue realizada por AnalyzeTransactionAsync
        return "approved";
    }

    public async Task<decimal> GetDailyTotalAsync(Guid sourceAccountId, DateTime targetDate)
    {
        var existingTransaction = await _context.TransactionDays
            .Where(td => td.SourceAccountId == sourceAccountId)
            .Where(td => td.TransactionDate.Date == targetDate.Date)
            .FirstOrDefaultAsync();

        return existingTransaction?.TotalValue ?? 0;
    }
}