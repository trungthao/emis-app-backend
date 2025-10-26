using System.Text.Json;
using Confluent.Kafka;
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Kafka.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMIS.EventBus.Kafka.Kafka;

/// <summary>
/// Background service for consuming Kafka events
/// </summary>
public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly Dictionary<string, Type> _eventTypeMapping;
    private IConsumer<string, string>? _consumer;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        KafkaConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
        _eventTypeMapping = new Dictionary<string, Type>();
    }

    /// <summary>
    /// Register event type mapping for deserialization
    /// </summary>
    public void RegisterEventType<TEvent>(string eventType) where TEvent : class, IEvent
    {
        _eventTypeMapping[eventType] = typeof(TEvent);
        _logger.LogInformation("Event type registered: {EventType} -> {TypeName}", eventType, typeof(TEvent).Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration.BootstrapServers,
            GroupId = _configuration.GroupId,
            ClientId = _configuration.ClientId,
            AutoOffsetReset = ParseAutoOffsetReset(_configuration.AutoOffsetReset),
            EnableAutoCommit = _configuration.EnableAutoCommit,
            EnableAutoOffsetStore = false, // Manual offset management
            // Add timeout to prevent blocking during startup
            SocketTimeoutMs = 10000,
        };

        // Add security configuration if provided
        if (!string.IsNullOrEmpty(_configuration.SecurityProtocol))
        {
            consumerConfig.SecurityProtocol = ParseSecurityProtocol(_configuration.SecurityProtocol);
        }

        if (!string.IsNullOrEmpty(_configuration.SaslMechanism))
        {
            consumerConfig.SaslMechanism = ParseSaslMechanism(_configuration.SaslMechanism);
            consumerConfig.SaslUsername = _configuration.SaslUsername;
            consumerConfig.SaslPassword = _configuration.SaslPassword;
        }

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

        // Subscribe to all registered event types
        if (_eventTypeMapping.Any())
        {
            var topics = _eventTypeMapping.Keys.ToList();
            _consumer.Subscribe(topics);
            _logger.LogInformation("Kafka consumer subscribed to topics: {Topics}", string.Join(", ", topics));

            // Yield to allow Kestrel to bind port before consuming messages
            await Task.Yield();
        }
        else
        {
            _logger.LogWarning("No event types registered. Consumer will not subscribe to any topics.");
            return;
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null)
                    {
                        await ProcessMessageAsync(consumeResult, stoppingToken);

                        // Commit offset after successful processing
                        _consumer.Commit(consumeResult);
                        _consumer.StoreOffset(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing message");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        var topic = consumeResult.Topic;
        var message = consumeResult.Message;

        _logger.LogInformation(
            "Message received. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}",
            topic, consumeResult.Partition, consumeResult.Offset, message.Key);

        // Get event type from mapping
        if (!_eventTypeMapping.TryGetValue(topic, out var eventType))
        {
            _logger.LogWarning("No event type mapping found for topic: {Topic}", topic);
            return;
        }

        try
        {
            // Deserialize event
            var @event = JsonSerializer.Deserialize(message.Value, eventType, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (@event == null)
            {
                _logger.LogWarning("Failed to deserialize event from topic: {Topic}", topic);
                return;
            }

            // Find and invoke handlers
            await InvokeHandlersAsync(eventType, @event, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from topic {Topic}: {Message}", topic, message.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event from topic {Topic}", topic);
            throw; // Re-throw to prevent commit
        }
    }

    private async Task InvokeHandlersAsync(Type eventType, object @event, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Get all registered handlers for this event type
        var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = scope.ServiceProvider.GetServices(handlerInterfaceType);

        if (!handlers.Any())
        {
            _logger.LogWarning("No handlers registered for event type: {EventType}", eventType.Name);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerInterfaceType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = handleMethod.Invoke(handler, new object[] { @event, cancellationToken }) as Task;
                    if (task != null)
                    {
                        await task;
                        _logger.LogInformation("Event handled successfully by {Handler}", handler?.GetType().Name ?? "Unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking handler {Handler} for event type {EventType}",
                    handler?.GetType().Name ?? "Unknown", eventType.Name);
                throw; // Re-throw to prevent commit
            }
        }
    }

    private static AutoOffsetReset ParseAutoOffsetReset(string autoOffsetReset) =>
        autoOffsetReset.ToLowerInvariant() switch
        {
            "earliest" => AutoOffsetReset.Earliest,
            "latest" => AutoOffsetReset.Latest,
            "error" => AutoOffsetReset.Error,
            _ => AutoOffsetReset.Earliest
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
