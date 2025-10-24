using Application.DTOs;
using Application.Interface;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services;

public class KafkaConsumerService : BackgroundService, IKafkaConsumerService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IFraudAnalysisService _fraudAnalysisService;
    private readonly IKafkaService _kafkaService;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _transactionEventsTopic;

    public KafkaConsumerService(
        IConfiguration configuration,
        IFraudAnalysisService fraudAnalysisService,
        IKafkaService kafkaService,
        ILogger<KafkaConsumerService> logger)
    {
        _fraudAnalysisService = fraudAnalysisService;
        _kafkaService = kafkaService;
        _logger = logger;
        _transactionEventsTopic = configuration["Kafka:TransactionEventsTopic"] ?? "transaction-events";

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "anti-fraud-service-group",
            ClientId = "anti-fraud-service-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartConsumingAsync(stoppingToken);
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_transactionEventsTopic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                    
                    if (consumeResult?.Message != null)
                    {
                        await ProcessTransactionEventAsync(consumeResult.Message);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogWarning("Topic '{Topic}' does not exist yet. Waiting for Transaction service to create it...", _transactionEventsTopic);
                    await Task.Delay(5000, cancellationToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer");
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    private async Task ProcessTransactionEventAsync(Message<string, string> message)
    {
        try
        {
            Console.WriteLine("MESSAGE RECEIVED FROM KAFKA");
            _logger.LogInformation("Raw event from transaction-events: {RawMessage}", message.Value);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var transactionEvent = JsonSerializer.Deserialize<TransactionCreatedEvent>(message.Value, options);
            if (transactionEvent == null)
            {
                return;
            }

            var analysisResult = await _fraudAnalysisService.AnalyzeTransactionAsync(transactionEvent);

            var statusEvent = new TransactionStatusEvent(
                transactionEvent.TransactionExternalId,
                analysisResult.IsApproved ? "approved" : "rejected",
                analysisResult.Reason,
                DateTime.UtcNow
            );

            await _kafkaService.PublishTransactionStatusAsync(statusEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction event");
            throw;
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}