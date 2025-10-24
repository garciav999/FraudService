using Application.DTOs;

namespace Application.Interface;

public interface IKafkaService
{
    Task PublishTransactionStatusAsync(TransactionStatusEvent statusEvent);
}

public interface IKafkaConsumerService
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}