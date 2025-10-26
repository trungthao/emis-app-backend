namespace EMIS.EventBus.Kafka.Configuration;

/// <summary>
/// Configuration for Kafka event bus
/// </summary>
public class KafkaConfiguration
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Kafka bootstrap servers (comma-separated)
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Consumer group ID
    /// </summary>
    public string GroupId { get; set; } = "emis-event-bus";

    /// <summary>
    /// Enable auto commit (default: false for manual commit control)
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// Auto offset reset (earliest, latest, none)
    /// </summary>
    public string AutoOffsetReset { get; set; } = "earliest";

    /// <summary>
    /// Client ID
    /// </summary>
    public string ClientId { get; set; } = "emis-service";

    /// <summary>
    /// Security protocol (plaintext, ssl, sasl_plaintext, sasl_ssl)
    /// </summary>
    public string? SecurityProtocol { get; set; }

    /// <summary>
    /// SASL mechanism (if using SASL)
    /// </summary>
    public string? SaslMechanism { get; set; }

    /// <summary>
    /// SASL username (if using SASL)
    /// </summary>
    public string? SaslUsername { get; set; }

    /// <summary>
    /// SASL password (if using SASL)
    /// </summary>
    public string? SaslPassword { get; set; }

    /// <summary>
    /// Request timeout in milliseconds
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Number of retries for failed produce
    /// </summary>
    public int MessageSendMaxRetries { get; set; } = 3;

    /// <summary>
    /// Acks required (0, 1, all)
    /// </summary>
    public string Acks { get; set; } = "all";

    /// <summary>
    /// Compression type (none, gzip, snappy, lz4, zstd)
    /// </summary>
    public string CompressionType { get; set; } = "lz4";

    /// <summary>
    /// Enable idempotence for exactly-once semantics
    /// </summary>
    public bool EnableIdempotence { get; set; } = true;
}
