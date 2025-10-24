using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Application.Interface;
using Application.DTOs;
using Application.Common;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lambda
{
    public class FraudAnalysisFunction
    {
        private readonly IServiceProvider _serviceProvider;

        public FraudAnalysisFunction()
        {
            var startup = new Startup();
            _serviceProvider = startup.Configure();
        }

        public record FraudAnalysisRequest(
            Guid TransactionExternalId,  // Cambié para coincidir
            Guid SourceAccountId,
            Guid TargetAccountId,        
            int TransferTypeId,          
            decimal Value,               
            string Status,
            Guid Id,
            DateTime OccurredAt,
            string EventType
        );

        public async Task<LambdaResponse<FraudAnalysisResult>> AnalyzeTransaction(FraudAnalysisRequest request)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var fraudAnalysisService = scope.ServiceProvider.GetRequiredService<IFraudAnalysisService>();

                var transactionEvent = new TransactionCreatedEvent(
                    request.TransactionExternalId,
                    request.SourceAccountId,
                    request.TargetAccountId,      
                    request.TransferTypeId,       
                    request.Value,                
                    request.Status,
                    request.Id,
                    request.OccurredAt,
                    request.EventType
                );

                var result = await fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

                return ResponseBuilder.Ok(result, "Fraud analysis completed successfully");
            }
            catch (Exception ex)
            {
                return ResponseBuilder.Error<FraudAnalysisResult>("Error during fraud analysis", ex);
            }
        }
    }
}