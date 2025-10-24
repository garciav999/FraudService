using Application.DTOs;
using Application.Interface;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class FraudAnalysisService : IFraudAnalysisService
{
    private readonly ILogger<FraudAnalysisService> _logger;
    private readonly ITransactionDayRepository _transactionDayRepository;

    public FraudAnalysisService(
        ILogger<FraudAnalysisService> logger, 
        ITransactionDayRepository transactionDayRepository)
    {
        _logger = logger;
        _transactionDayRepository = transactionDayRepository;
    }

    public async Task<FraudAnalysisResult> AnalyzeTransactionAsync(TransactionCreatedEvent transactionEvent)
    {
        var riskFactors = new List<string>();
        var isApproved = true;
        var reason = "Transaction approved";

        var transactionDate = transactionEvent.OccurredAt;

        if (transactionEvent.Value > 2500)
        {
            isApproved = false;
            reason = "Transaction rejected - Individual amount exceeds $2,500 limit";
            riskFactors.Add($"Individual amount ${transactionEvent.Value:N2} exceeds $2,500 limit");
            return new FraudAnalysisResult(isApproved, reason, 100, riskFactors);
        }

        var dailyTotal = await GetDailyTotalAsync(transactionEvent.SourceAccountId, transactionDate);
        var newDailyTotal = dailyTotal + transactionEvent.Value;

        if (newDailyTotal > 20500)
        {
            isApproved = false;
            reason = $"Transaction rejected - Daily total would exceed $20,500 limit (current: ${dailyTotal:N2}, new total: ${newDailyTotal:N2})";
            riskFactors.Add($"Daily total ${newDailyTotal:N2} would exceed $20,500 limit");
            return new FraudAnalysisResult(isApproved, reason, 100, riskFactors);
        }

        if (isApproved)
        {
            await _transactionDayRepository.UpsertTransactionDayAsync(
                transactionDate, 
                transactionEvent.SourceAccountId, 
                transactionEvent.Value);
        }

        return new FraudAnalysisResult(isApproved, reason, 0, riskFactors);
    }

    private async Task<decimal> GetDailyTotalAsync(Guid sourceAccountId, DateTime transactionDate)
    {
        try
        {
            var targetDate = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);
            return await _transactionDayRepository.GetDailyTotalAsync(sourceAccountId, targetDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily total for account {AccountId} on date {Date}", 
                sourceAccountId, transactionDate);
            return 0;
        }
    }
}