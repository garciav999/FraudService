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
        _logger.LogInformation("Analyzing transaction {TransactionId} for fraud", transactionEvent.TransactionExternalId);

        var riskFactors = new List<string>();
        var isApproved = true;
        var reason = "Transaction approved";

        // Usar OccurredAt como fecha de transacción
        var transactionDate = transactionEvent.OccurredAt;

        // REGLA 1: Transacción individual excede $2,500
        if (transactionEvent.Value > 2500)
        {
            isApproved = false;
            reason = "Transaction rejected - Individual amount exceeds $2,500 limit";
            riskFactors.Add($"Individual amount ${transactionEvent.Value:N2} exceeds $2,500 limit");
            
            _logger.LogWarning("Transaction {TransactionId} rejected - Amount ${Amount} exceeds individual limit of $2,500", 
                transactionEvent.TransactionExternalId, transactionEvent.Value);
            
            return new FraudAnalysisResult(isApproved, reason, 100, riskFactors);
        }

        // REGLA 2: Verificar si el total del día excedería $20,500
        var dailyTotal = await GetDailyTotalAsync(transactionEvent.SourceAccountId, transactionDate);
        var newDailyTotal = dailyTotal + transactionEvent.Value;

        if (newDailyTotal > 20500)
        {
            isApproved = false;
            reason = $"Transaction rejected - Daily total would exceed $20,500 limit (current: ${dailyTotal:N2}, new total: ${newDailyTotal:N2})";
            riskFactors.Add($"Daily total ${newDailyTotal:N2} would exceed $20,500 limit");
            
            _logger.LogWarning("Transaction {TransactionId} rejected - Daily total ${NewTotal} would exceed $20,500 limit", 
                transactionEvent.TransactionExternalId, newDailyTotal);
            
            return new FraudAnalysisResult(isApproved, reason, 100, riskFactors);
        }

        // Si pasa todas las validaciones, ACTUALIZAR el total del día
        if (isApproved)
        {
            await _transactionDayRepository.UpsertTransactionDayAsync(
                transactionDate, 
                transactionEvent.SourceAccountId, 
                transactionEvent.Value);
                
            _logger.LogInformation("Transaction {TransactionId} approved and daily total updated - Amount: ${Amount}, New daily total: ${DailyTotal}", 
                transactionEvent.TransactionExternalId, transactionEvent.Value, newDailyTotal);
        }

        return new FraudAnalysisResult(isApproved, reason, 0, riskFactors);
    }

    private async Task<decimal> GetDailyTotalAsync(Guid sourceAccountId, DateTime transactionDate)
    {
        try
        {
            // Usar el mismo método del repositorio para obtener el total del día
            // Pero necesitamos crear un método de consulta que no haga upsert
            var targetDate = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);
            
            return await _transactionDayRepository.GetDailyTotalAsync(sourceAccountId, targetDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily total for account {AccountId} on date {Date}", 
                sourceAccountId, transactionDate);
            // En caso de error, asumir 0 para no bloquear transacciones por errores de consulta
            return 0;
        }
    }
}