
namespace AtlasPay.Notification.Worker.Configuration;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;

    public int MaxRetries { get; set; } = 3;

    public Dictionary<string, KafkaConsumerSettings> Consumers { get; set; } = [];
}

public sealed class KafkaConsumerSettings
{
    public string Topic { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public string DeadLetterTopic { get; set; } = string.Empty;
}

public sealed class Validations
{
    public static bool ValidateKafkaSettings(KafkaSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.BootstrapServers)
        && settings.MaxRetries > 0
        && settings.Consumers is not null
        && settings.Consumers.ContainsKey("Finance")
        && settings.Consumers.ContainsKey("Identity")
        && settings.Consumers.Values.All(x =>
            x is not null
            && !string.IsNullOrWhiteSpace(x.Topic)
            && !string.IsNullOrWhiteSpace(x.GroupId)
            && !string.IsNullOrWhiteSpace(x.DeadLetterTopic));
}