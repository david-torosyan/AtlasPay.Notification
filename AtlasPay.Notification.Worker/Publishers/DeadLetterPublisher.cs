
using AtlasPay.Notification.Worker.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text;

namespace AtlasPay.Notification.Worker.Publishers;

public sealed class DeadLetterPublisher : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _settings;

    public DeadLetterPublisher(IOptions<KafkaSettings> options)
    {
        _settings = options.Value;

        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                EnableIdempotence = true,
                Acks = Acks.All
            }).Build();
    }

    public async Task PublishAsync(
        string consumerName,
        ConsumeResult<string, string> result,
        Exception exception,
        CancellationToken ct = default)
    {
        var topic = _settings.Consumers[consumerName].DeadLetterTopic;

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
            Encoding.UTF8.GetBytes(result.Topic));

        message.Headers.Add(
            "original-partition",
            Encoding.UTF8.GetBytes(
                result.Partition.Value.ToString()));

        message.Headers.Add(
            "original-offset",
            Encoding.UTF8.GetBytes(
                result.Offset.Value.ToString()));

        message.Headers.Add(
            "error-type",
            Encoding.UTF8.GetBytes(
                exception.GetType().Name));

        await _producer.ProduceAsync(
            topic,
            message,
            ct);
    }

    public void Dispose() => _producer.Dispose();
}
