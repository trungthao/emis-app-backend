using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Kafka.Configuration;
using EMIS.EventBus.Kafka.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMIS.EventBus.Kafka.Extensions;

/// <summary>
/// Extension methods for registering Kafka EventBus in DI container
/// </summary>
public static class KafkaEventBusExtensions
{
    /// <summary>
    /// Add Kafka EventBus to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKafkaEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind Kafka configuration
        var kafkaConfig = new KafkaConfiguration();
        configuration.GetSection(KafkaConfiguration.SectionName).Bind(kafkaConfig);
        services.AddSingleton(kafkaConfig);

        // Register EventBus
        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }

    /// <summary>
    /// Add Kafka EventBus with custom configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKafkaEventBus(
        this IServiceCollection services,
        Action<KafkaConfiguration> configureOptions)
    {
        var kafkaConfig = new KafkaConfiguration();
        configureOptions(kafkaConfig);
        services.AddSingleton(kafkaConfig);

        // Register EventBus
        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }

    /// <summary>
    /// Add Kafka consumer as hosted service
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureConsumer">Consumer configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddKafkaConsumer(
        this IServiceCollection services,
        Action<KafkaConsumerService>? configureConsumer = null)
    {
        // Register consumer as singleton to allow configuration
        services.AddSingleton<KafkaConsumerService>();

        // Register as hosted service
        services.AddHostedService(provider =>
        {
            var consumerService = provider.GetRequiredService<KafkaConsumerService>();
            configureConsumer?.Invoke(consumerService);
            return consumerService;
        });

        return services;
    }

    /// <summary>
    /// Add event handler to the service collection
    /// </summary>
    /// <typeparam name="TEvent">Event type</typeparam>
    /// <typeparam name="TEventHandler">Event handler type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddEventHandler<TEvent, TEventHandler>(
        this IServiceCollection services)
        where TEvent : class, IEvent
        where TEventHandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, TEventHandler>();
        return services;
    }
}
