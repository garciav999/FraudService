using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Application.Interface;
using Application.Common;

namespace Lambda
{
    public class KafkaConsumerFunction
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerFunction()
        {
            var startup = new Startup();
            _serviceProvider = startup.Configure();
        }

        public async Task<LambdaResponse<string>> StartConsumer()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var consumerService = scope.ServiceProvider.GetRequiredService<IKafkaConsumerService>();
                
                var cancellationTokenSource = new CancellationTokenSource();
                
                var timeout = TimeSpan.FromMinutes(15); 
                cancellationTokenSource.CancelAfter(timeout);
                
                await consumerService.StartConsumingAsync(cancellationTokenSource.Token);
                
                return ResponseBuilder.Ok("Kafka consumer completed successfully");
            }
            catch (Exception ex)
            {
                return ResponseBuilder.Error<string>("Error starting Kafka consumer", ex);
            }
        }
    }
}