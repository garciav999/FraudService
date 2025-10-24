using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Application.Commands;
using Application.Common;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lambda
{
    public class Function
    {
        private readonly IServiceProvider _serviceProvider;

        public Function()
        {
            var startup = new Startup();
            _serviceProvider = startup.Configure();
        }

        public record UpsertTransactionDayRequest(Guid SourceAccountId, DateTime TransactionDate, decimal Value);

        public async Task<LambdaResponse<string>> Handler(UpsertTransactionDayRequest request)
        {
            try
            {
                if (request.Value < 0) 
                {
                    return ResponseBuilder.Error<string>("Value must be non-negative.");
                }

                using var scope = _serviceProvider.CreateScope();

                var commands = scope.ServiceProvider.GetRequiredService<TransactionDayCommands>();

                var result = await commands.UpsertTransactionDayAsync(
                    request.TransactionDate,
                    request.SourceAccountId,
                    request.Value
                );

                return ResponseBuilder.Ok(result, "Transaction day processed successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseBuilder.Error<string>("Invalid input", ex);
            }
            catch (Exception ex)
            {
                return ResponseBuilder.Error<string>("An unexpected error occurred", ex);
            }
        }
    }
}
