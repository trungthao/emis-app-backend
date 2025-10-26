# EMIS.EventBus.Kafka

**Apache Kafka implementation** of EMIS.EventBus abstractions for production-grade event-driven microservices.

> ⚡ **Production-ready** Kafka implementation with 3-broker cluster, idempotent producers, and manual offset management.

## Features

- ✅ **High Availability**: 3-broker Kafka cluster with replication factor 3
- ✅ **Exactly-Once Semantics**: Idempotent producers + manual offset control
- ✅ **Auto Retry**: Configurable retry mechanism with exponential backoff
- ✅ **Compression**: LZ4 compression for bandwidth optimization
- ✅ **Schema Registry**: Integration with Confluent Schema Registry
- ✅ **Monitoring Ready**: Comprehensive logging + Kafka UI
- ✅ **Production Hardened**: Security (SASL/SSL), error handling, DLQ support

## Architecture

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│ TeacherService  │  Pub    │  Kafka Cluster   │   Sub   │  AuthService    │
│                 ├────────▶│  (3 brokers)     ├────────▶│                 │
│ IEventBus impl  │         │  + Zookeeper     │         │ EventHandler    │
└─────────────────┘         └──────────────────┘         └─────────────────┘
                                     │
                            ┌────────▼────────┐
                            │  Kafka UI       │
                            │  localhost:8080 │
                            └─────────────────┘
```

## Installation

Add reference to your project:

```bash
# From your service project (e.g., Teacher.API)
dotnet add reference ../../../Shared/EMIS.EventBus.Kafka/EMIS.EventBus.Kafka.csproj
```

This automatically includes `EMIS.EventBus` as a transitive dependency.

## Configuration

Add Kafka configuration to `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092,localhost:9093,localhost:9094",
    "GroupId": "emis-auth-service",
    "ClientId": "auth-service-1",
    "AutoOffsetReset": "earliest",
    "EnableAutoCommit": false,
    "Acks": "all",
    "CompressionType": "lz4",
    "EnableIdempotence": true,
    "RequestTimeoutMs": 30000,
    "MessageSendMaxRetries": 3
  }
}
```

### Configuration Reference

| Property | Default | Description |
|----------|---------|-------------|
| `BootstrapServers` | `localhost:9092` | Comma-separated Kafka brokers |
| `GroupId` | `emis-event-bus` | Consumer group ID (unique per service) |
| `ClientId` | `emis-service` | Client identifier for monitoring |
| `AutoOffsetReset` | `earliest` | Start from beginning if no offset (earliest/latest/none) |
| `EnableAutoCommit` | `false` | Manual commit for reliability |
| `Acks` | `all` | Producer acknowledgment (0/1/all) |
| `CompressionType` | `lz4` | Compression (none/gzip/snappy/lz4/zstd) |
| `EnableIdempotence` | `true` | Exactly-once semantics |
| `RequestTimeoutMs` | `30000` | Request timeout in milliseconds |
| `MessageSendMaxRetries` | `3` | Number of retries on failure |

### Security Configuration (Production)

```json
{
  "Kafka": {
    "SecurityProtocol": "sasl_ssl",
    "SaslMechanism": "SCRAM-SHA-256",
    "SaslUsername": "your-username",
    "SaslPassword": "your-password"
  }
}
```

## Usage

### 1. Producer (Publishing Events)

Register EventBus in `Program.cs`:

```csharp
using EMIS.EventBus.Kafka.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Kafka EventBus
builder.Services.AddKafkaEventBus(builder.Configuration);

var app = builder.Build();
app.Run();
```

Publish events in your handler:

```csharp
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Events;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherResponse>
{
    private readonly IEventBus _eventBus;
    private readonly ITeacherRepository _repository;
    private readonly ILogger<CreateTeacherCommandHandler> _logger;

    public CreateTeacherCommandHandler(
        IEventBus eventBus, 
        ITeacherRepository repository,
        ILogger<CreateTeacherCommandHandler> logger)
    {
        _eventBus = eventBus;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TeacherResponse> Handle(
        CreateTeacherCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Create teacher in database
        var teacher = new Teacher
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Subject = request.Subject
        };

        await _repository.CreateAsync(teacher);

        _logger.LogInformation("Teacher created: {TeacherId}", teacher.Id);

        // 2. Publish event for AuthService
        var @event = new TeacherCreatedEvent
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            PhoneNumber = teacher.PhoneNumber,
            Subject = teacher.Subject,
            DefaultPassword = GenerateSecurePassword()
        };

        await _eventBus.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("TeacherCreatedEvent published: {EventId}", @event.EventId);

        return MapToResponse(teacher);
    }

    private string GenerateSecurePassword()
    {
        // Use a secure password generator
        return $"Teacher{Guid.NewGuid().ToString("N")[..8]}!";
    }
}
```

### 2. Consumer (Handling Events)

Create event handler:

```csharp
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Events;
using MediatR;

public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherCreatedEventHandler> _logger;

    public TeacherCreatedEventHandler(
        IMediator mediator, 
        ILogger<TeacherCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(
        TeacherCreatedEvent @event, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TeacherCreatedEvent: {EventId}, TeacherId: {TeacherId}", 
            @event.EventId, 
            @event.TeacherId);

        try
        {
            // Create authentication account
            var registerCommand = new RegisterCommand
            {
                Username = @event.Email,
                Email = @event.Email,
                Password = @event.DefaultPassword,
                FullName = @event.FullName,
                PhoneNumber = @event.PhoneNumber,
                Roles = new List<string> { "Teacher" }
            };

            var result = await _mediator.Send(registerCommand, cancellationToken);

            _logger.LogInformation(
                "Account created for teacher {TeacherId}: UserId={UserId}", 
                @event.TeacherId, 
                result.UserId);

            // TODO: Update teacher with UserId (optional saga pattern)
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Failed to create account for teacher {TeacherId}", 
                @event.TeacherId);
            
            // Re-throw to prevent offset commit (message will be retried)
            throw;
        }
    }
}
```

Register consumer in `Program.cs`:

```csharp
using EMIS.EventBus.Kafka.Extensions;
using EMIS.EventBus.Events;

var builder = WebApplication.CreateBuilder(args);

// Add Kafka EventBus
builder.Services.AddKafkaEventBus(builder.Configuration);

// Register event handlers
builder.Services.AddEventHandler<TeacherCreatedEvent, TeacherCreatedEventHandler>();
builder.Services.AddEventHandler<ParentCreatedEvent, ParentCreatedEventHandler>();
builder.Services.AddEventHandler<StudentCreatedEvent, StudentCreatedEventHandler>();

// Add consumer background service
builder.Services.AddKafkaConsumer(consumer =>
{
    // Register topics to consume
    consumer.RegisterEventType<TeacherCreatedEvent>("emis.auth.teacher.created");
    consumer.RegisterEventType<ParentCreatedEvent>("emis.auth.parent.created");
    consumer.RegisterEventType<StudentCreatedEvent>("emis.auth.student.created");
});

var app = builder.Build();
app.Run();
```

## Kafka Cluster Setup

The Docker Compose configuration includes:

```yaml
services:
  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    ports: ["2181:2181"]

  kafka-1:
    image: confluentinc/cp-kafka:7.5.0
    ports: ["9092:9092"]

  kafka-2:
    image: confluentinc/cp-kafka:7.5.0
    ports: ["9093:9093"]

  kafka-3:
    image: confluentinc/cp-kafka:7.5.0
    ports: ["9094:9094"]

  schema-registry:
    image: confluentinc/cp-schema-registry:7.5.0
    ports: ["8081:8081"]

  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    ports: ["8080:8080"]
```

Start Kafka cluster:

```bash
docker compose up -d zookeeper kafka-1 kafka-2 kafka-3 schema-registry kafka-ui
```

## Topic Management

### Creating Topics

**Using Kafka UI** (recommended):
1. Open http://localhost:8080
2. Go to "Topics" → "Create Topic"
3. Configure partitions, replication, retention

**Using CLI**:

```bash
# Access Kafka container
docker exec -it emis-kafka-1 bash

# Create topic
kafka-topics --create \
  --bootstrap-server localhost:9092 \
  --topic emis.auth.teacher.created \
  --partitions 10 \
  --replication-factor 3 \
  --config min.insync.replicas=2 \
  --config retention.ms=604800000

# List topics
kafka-topics --list --bootstrap-server localhost:9092

# Describe topic
kafka-topics --describe \
  --bootstrap-server localhost:9092 \
  --topic emis.auth.teacher.created
```

### Recommended Topic Configuration

For **Vietnam-wide scale** (25,000 schools, 25M users):

```bash
# Teacher creation events (~10,000 teachers/day)
Topic: emis.auth.teacher.created
Partitions: 10
Replication: 3
Min ISR: 2
Retention: 7 days (604800000 ms)

# Parent creation events (~20,000 parents/day)
Topic: emis.auth.parent.created
Partitions: 15
Replication: 3
Min ISR: 2
Retention: 7 days

# Student creation events (peaks during enrollment: 100K/day)
Topic: emis.auth.student.created
Partitions: 20
Replication: 3
Min ISR: 2
Retention: 7 days
```

## Error Handling

### Automatic Retries

Producer retries are automatic:
- `MessageSendMaxRetries`: 3
- Exponential backoff
- Idempotent mode prevents duplicates

### Dead Letter Queue (DLQ)

Implement DLQ for messages that fail after all retries:

```csharp
public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken ct)
{
    const int maxRetries = 3;
    var retryCount = 0;

    while (retryCount < maxRetries)
    {
        try
        {
            await ProcessEventAsync(@event, ct);
            return; // Success
        }
        catch (Exception ex)
        {
            retryCount++;
            _logger.LogWarning(
                "Retry {Retry}/{MaxRetries} for event {EventId}: {Error}",
                retryCount, maxRetries, @event.EventId, ex.Message);

            if (retryCount >= maxRetries)
            {
                // Send to DLQ
                await PublishToDLQAsync(@event, ex, ct);
                
                // Don't throw - commit offset to prevent infinite retries
                _logger.LogError(
                    "Event {EventId} moved to DLQ after {Retries} retries",
                    @event.EventId, maxRetries);
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), ct);
        }
    }
}
```

## Monitoring

### Kafka UI

Access: http://localhost:8080

Features:
- 📊 View topics, partitions, consumer groups
- 🔍 Inspect messages (browse, search, filter)
- 📈 Monitor broker metrics (throughput, lag, errors)
- ⚙️ Manage topics (create, update, delete)
- 👥 View consumer group lag

### Logging

KafkaEventBus logs important events:

```
[INF] Event published successfully. Topic: emis.auth.teacher.created, EventId: abc123, Partition: 3, Offset: 12045
[INF] Message received. Topic: emis.auth.teacher.created, Partition: 3, Offset: 12045
[INF] Event handled successfully by TeacherCreatedEventHandler
```

### Metrics (TODO)

For production, integrate Prometheus metrics:

```csharp
// Producer metrics
- kafka_producer_record_send_total
- kafka_producer_record_error_total
- kafka_producer_record_retry_total

// Consumer metrics
- kafka_consumer_records_consumed_total
- kafka_consumer_lag
- kafka_consumer_commit_latency_avg
```

## Production Checklist

- [ ] Enable SASL/SSL authentication
- [ ] Configure network policies (firewall, security groups)
- [ ] Set up monitoring (Prometheus + Grafana)
- [ ] Configure alerting (consumer lag, error rate)
- [ ] Implement DLQ pattern
- [ ] Set appropriate retention policies
- [ ] Scale consumers based on partition count
- [ ] Test failover scenarios
- [ ] Document runbooks for common issues
- [ ] Configure log aggregation (ELK, Loki)

## Troubleshooting

### Consumer Lag

Check consumer group lag in Kafka UI or CLI:

```bash
kafka-consumer-groups --bootstrap-server localhost:9092 \
  --describe --group emis-auth-service
```

Solution: Scale consumers (add more instances with same GroupId)

### Message Not Consumed

1. Check topic exists: `kafka-topics --list`
2. Verify consumer subscribed: Check logs for "subscribed to topics"
3. Check consumer group: Ensure GroupId is unique per service
4. Verify message format: Check JSON serialization

### Producer Timeout

1. Increase `RequestTimeoutMs` in configuration
2. Check broker health: `docker compose ps`
3. Verify network connectivity
4. Check broker logs: `docker compose logs kafka-1`

## Dependencies

- **EMIS.EventBus** ^1.0.0 (abstractions)
- **Confluent.Kafka** 2.12.0
- **Microsoft.Extensions.Configuration.Abstractions** 9.0.10
- **Microsoft.Extensions.Configuration.Binder** 9.0.10
- **Microsoft.Extensions.DependencyInjection.Abstractions** 9.0.10
- **Microsoft.Extensions.Hosting.Abstractions** 9.0.10
- **Microsoft.Extensions.Logging.Abstractions** 9.0.10

## Performance Tips

1. **Batching**: Use `Linger.ms` for batching small messages
2. **Compression**: LZ4 or Snappy for best throughput/compression ratio
3. **Partitioning**: More partitions = higher parallelism
4. **Consumer Count**: Match partition count for max throughput
5. **Network**: Co-locate consumers with Kafka brokers (same region/AZ)

## License

Internal EMIS project - not for public distribution.
