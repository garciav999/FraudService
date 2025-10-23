using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Application.Commands;

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
        public record UpsertTransactionDayResponse(string Status);

        public async Task<UpsertTransactionDayResponse> Handler(UpsertTransactionDayRequest request)
        {
            if (request.Value < 0) throw new ArgumentException("Value must be non-negative.", nameof(request.Value));

            using var scope = _serviceProvider.CreateScope();

            var commands = scope.ServiceProvider.GetRequiredService<TransactionDayCommands>();

            var result = await commands.UpsertTransactionDayAsync(
                request.TransactionDate,
                request.SourceAccountId,
                request.Value
            );

            return new UpsertTransactionDayResponse(result);
        }
    }
}
