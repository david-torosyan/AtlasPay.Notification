
using AtlasPay.Notification.Worker.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AtlasPay.Notification.Worker.Kafka;

public sealed class KafkaConsumerFactory(
    IOptions<KafkaSettings> options)
{
    private readonly KafkaSettings _settings = options.Value;

    public IConsumer<string, string> Create(string consumerName)
    {
        var settings = _settings.Consumers[consumerName];

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        var consumer = new ConsumerBuilder<string, string>(config)
            .Build();

        consumer.Subscribe(settings.Topic);

        return consumer;
    }
}
