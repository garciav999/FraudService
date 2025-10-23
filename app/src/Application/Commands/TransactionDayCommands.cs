using Application.DTOs;
using Application.Interface;
using Domain.Entities;

namespace Application.Commands;

public class TransactionDayCommands
{
    private readonly ITransactionDayRepository _transactionDayRepository;

    public TransactionDayCommands(ITransactionDayRepository transactionDayRepository)
    {
        _transactionDayRepository = transactionDayRepository;
    }

    public async Task<string> UpsertTransactionDayAsync(DateTime transactionDate, Guid sourceAccountId, decimal value)
    {
        return await _transactionDayRepository.UpsertTransactionDayAsync(
            transactionDate,
            sourceAccountId,
            value);
    }
}