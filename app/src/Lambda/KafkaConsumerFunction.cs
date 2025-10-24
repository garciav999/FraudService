using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Application.Interface;
using Application.Common;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

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
                
                // En un entorno real, esto correría indefinidamente
                // Para Lambda, podrías ejecutarlo por un tiempo limitado
                var timeout = TimeSpan.FromMinutes(15); // Lambda max timeout
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