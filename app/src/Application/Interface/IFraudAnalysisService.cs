using Application.DTOs;

namespace Application.Interface;

public interface IFraudAnalysisService
{
    Task<FraudAnalysisResult> AnalyzeTransactionAsync(TransactionCreatedEvent transactionEvent);
}

public record FraudAnalysisResult(
    bool IsApproved,
    string Reason,
    decimal RiskScore,
    List<string> RiskFactors
);