
using Confluent.Kafka;

namespace AtlasPay.Notification.Worker.Publishers;

public sealed class DeadLetterPublisher : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public DeadLetterPublisher(IConfiguration configuration)
    {
        _topic = configuration["Kafka:DeadLetterTopic"]
            ?? throw new InvalidOperationException(
                "Kafka dead-letter topic is not configured.");

        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                EnableIdempotence = true,
                Acks = Acks.All
            }).Build();
    }

    public async Task PublishAsync(
        ConsumeResult<string, string> result,
        Exception exception,
        CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = new Headers()
        };

        if (result.Message.Headers is not null)
        {
            foreach (var header in result.Message.Headers)
            {
                message.Headers.Add(
                    header.Key,
                    header.GetValueBytes());
            }
        }

        message.Headers.Add(
            "original-topic",
            System.Text.Encoding.UTF8.GetBytes(result.Topic));

        message.Headers.Add(
            "original-partition",
            System.Text.Encoding.UTF8.GetBytes(
                result.Partition.Value.ToString()));

        message.Headers.Add(
            "original-offset",
            System.Text.Encoding.UTF8.GetBytes(
                result.Offset.Value.ToString()));

        message.Headers.Add(
            "error-type",
            System.Text.Encoding.UTF8.GetBytes(
                exception.GetType().Name));

        await _producer.ProduceAsync(
            _topic,
            message,
            ct);
    }

    public void Dispose() => _producer.Dispose();
}
