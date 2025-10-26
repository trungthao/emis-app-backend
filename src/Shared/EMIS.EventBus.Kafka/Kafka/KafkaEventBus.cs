using System.Text.Json;
using Confluent.Kafka;
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Kafka.Configuration;
using Microsoft.Extensions.Logging;

namespace EMIS.EventBus.Kafka.Kafka;

/// <summary>
/// Kafka implementation of IEventBus
/// </summary>
public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly Dictionary<Type, List<Type>> _eventHandlers;
    private readonly IServiceProvider _serviceProvider;

    public KafkaEventBus(
        KafkaConfiguration configuration,
        ILogger<KafkaEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventHandlers = new Dictionary<Type, List<Type>>();

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration.BootstrapServers,
            ClientId = configuration.ClientId,
            Acks = ParseAcks(configuration.Acks),
            MessageSendMaxRetries = configuration.MessageSendMaxRetries,
            EnableIdempotence = configuration.EnableIdempotence,
            CompressionType = ParseCompressionType(configuration.CompressionType),
            RequestTimeoutMs = configuration.RequestTimeoutMs,
        };

        // Add security configuration if provided
        if (!string.IsNullOrEmpty(configuration.SecurityProtocol))
        {
            producerConfig.SecurityProtocol = ParseSecurityProtocol(configuration.SecurityProtocol);
        }

        if (!string.IsNullOrEmpty(configuration.SaslMechanism))
        {
            producerConfig.SaslMechanism = ParseSaslMechanism(configuration.SaslMechanism);
            producerConfig.SaslUsername = configuration.SaslUsername;
            producerConfig.SaslPassword = configuration.SaslPassword;
        }

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger.LogInformation("Kafka EventBus initialized with bootstrap servers: {BootstrapServers}",
            configuration.BootstrapServers);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var topic = @event.EventType;
        var key = @event.EventId.ToString();
        var value = JsonSerializer.Serialize(@event, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        try
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = value,
                Headers = new Headers
                {
                    { "EventType", System.Text.Encoding.UTF8.GetBytes(@event.EventType) },
                    { "EventId", System.Text.Encoding.UTF8.GetBytes(@event.EventId.ToString()) },
                    { "Timestamp", System.Text.Encoding.UTF8.GetBytes(@event.Timestamp.ToString("O")) }
                }
            };

            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);

            _logger.LogInformation(
                "Event published successfully. Topic: {Topic}, EventId: {EventId}, Partition: {Partition}, Offset: {Offset}",
                topic, @event.EventId, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Failed to publish event. Topic: {Topic}, EventId: {EventId}, Error: {Error}",
                topic, @event.EventId, ex.Error.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error publishing event. Topic: {Topic}, EventId: {EventId}",
                topic, @event.EventId);
            throw;
        }
    }

    public void Subscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(TEventHandler);

        if (!_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType] = new List<Type>();
        }

        if (!_eventHandlers[eventType].Contains(handlerType))
        {
            _eventHandlers[eventType].Add(handlerType);
            _logger.LogInformation(
                "Event handler registered. Event: {EventType}, Handler: {HandlerType}",
                eventType.Name, handlerType.Name);
        }
    }

    public void Unsubscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(TEventHandler);

        if (_eventHandlers.ContainsKey(eventType))
        {
            _eventHandlers[eventType].Remove(handlerType);
            _logger.LogInformation(
                "Event handler unregistered. Event: {EventType}, Handler: {HandlerType}",
                eventType.Name, handlerType.Name);
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _logger.LogInformation("Kafka EventBus disposed");
    }

    private static Acks ParseAcks(string acks) => acks.ToLowerInvariant() switch
    {
        "0" => Acks.None,
        "1" => Acks.Leader,
        "all" => Acks.All,
        "-1" => Acks.All,
        _ => Acks.All
    };

    private static CompressionType ParseCompressionType(string compressionType) =>
        compressionType.ToLowerInvariant() switch
        {
            "none" => CompressionType.None,
            "gzip" => CompressionType.Gzip,
            "snappy" => CompressionType.Snappy,
            "lz4" => CompressionType.Lz4,
            "zstd" => CompressionType.Zstd,
            _ => CompressionType.Lz4
        };

    private static SecurityProtocol ParseSecurityProtocol(string securityProtocol) =>
        securityProtocol.ToLowerInvariant() switch
        {
            "plaintext" => SecurityProtocol.Plaintext,
            "ssl" => SecurityProtocol.Ssl,
            "sasl_plaintext" => SecurityProtocol.SaslPlaintext,
            "sasl_ssl" => SecurityProtocol.SaslSsl,
            _ => SecurityProtocol.Plaintext
        };

    private static SaslMechanism ParseSaslMechanism(string saslMechanism) =>
        saslMechanism.ToUpperInvariant() switch
        {
            "PLAIN" => SaslMechanism.Plain,
            "SCRAM-SHA-256" => SaslMechanism.ScramSha256,
            "SCRAM-SHA-512" => SaslMechanism.ScramSha512,
            "GSSAPI" => SaslMechanism.Gssapi,
            "OAUTHBEARER" => SaslMechanism.OAuthBearer,
            _ => SaslMechanism.Plain
        };
}
