using Application.DTOs;
using Application.Interface;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services;

public class KafkaService : IKafkaService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaService> _logger;
    private readonly string _transactionStatusTopic;

    public KafkaService(IConfiguration configuration, ILogger<KafkaService> logger)
    {
        _logger = logger;
        _transactionStatusTopic = configuration["Kafka:TransactionStatusTopic"] ?? "transaction-status-events";

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            ClientId = "anti-fraud-service-producer"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishTransactionStatusAsync(TransactionStatusEvent statusEvent)
    {
        try
        {
            var message = JsonSerializer.Serialize(statusEvent);
            var kafkaMessage = new Message<string, string>
            {
                Key = statusEvent.TransactionId.ToString(),
                Value = message
            };

            var result = await _producer.ProduceAsync(_transactionStatusTopic, kafkaMessage);
            
            _logger.LogInformation("Published transaction status event for transaction {TransactionId} to topic {Topic}", 
                statusEvent.TransactionId, _transactionStatusTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing transaction status event for transaction {TransactionId}", 
                statusEvent.TransactionId);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}