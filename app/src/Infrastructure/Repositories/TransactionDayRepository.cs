using Application.Interface;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionDayRepository : ITransactionDayRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionDayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(TransactionDay transactionDay)
    {
        await _context.TransactionDays.AddAsync(transactionDay);
        await _context.SaveChangesAsync();
    }

    public async Task<string> UpsertTransactionDayAsync(DateTime transactionDate, Guid sourceAccountId, decimal value)
    {
        var targetDate = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);
        
        Console.WriteLine($"Input date: {transactionDate:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Target date: {targetDate:yyyy-MM-dd HH:mm:ss}");

        var existingTransaction = await _context.TransactionDays
            .Where(td => td.SourceAccountId == sourceAccountId)
            .Where(td => td.TransactionDate.Date == targetDate.Date)
            .FirstOrDefaultAsync();

        decimal currentTotalValue = existingTransaction?.TotalValue ?? 0;
        if (value > 2500 || (currentTotalValue + value) > 20500)
        {
            return "rejected";
        }

        if (existingTransaction != null)
        {
            existingTransaction.AddToTotalValue(value);
        }
        else
        {
            var newTransactionDay = new TransactionDay(targetDate, sourceAccountId, value);
            Console.WriteLine($"Creating new transaction with date: {newTransactionDay.TransactionDate:yyyy-MM-dd HH:mm:ss}");
            await _context.TransactionDays.AddAsync(newTransactionDay);
        }

        await _context.SaveChangesAsync();
        return "approved";
    }

}