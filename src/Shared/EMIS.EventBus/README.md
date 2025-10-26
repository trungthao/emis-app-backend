# EMIS.EventBus

**Abstraction layer** for event-driven communication in EMIS (Educational Management Information System) microservices architecture.

> 🎯 **This library only contains abstractions** - interfaces and base classes. 
> - For **domain events**, use `EMIS.Contracts`
> - For **implementations**, use `EMIS.EventBus.Kafka` or `EMIS.EventBus.RabbitMQ`

## Features

- ✅ **Message Broker Agnostic**: Switch between Kafka, RabbitMQ, or custom implementations
- ✅ **Type-Safe Events**: Strong typing with generic constraints
- ✅ **Clean Architecture**: Separation of abstractions and implementations
- ✅ **Extensible**: Easy to add new event types and handlers
- ✅ **Zero Dependencies**: Pure abstractions, no external packages

## Architecture

```
┌─────────────────────┐
│  EMIS.EventBus      │  ← Core abstractions (IEvent, IEventBus, IEventHandler)
│  (this library)     │
└──────────┬──────────┘
           │
    ┌──────┴──────────────────────┬──────────────┐
    │                             │              │
┌───▼─────────────┐   ┌───────────▼──────┐   ┌──▼──────────┐
│ EMIS.Contracts  │   │ EMIS.EventBus    │   │ EMIS.EventBus│
│ (Events, DTOs)  │   │ .Kafka           │   │ .RabbitMQ    │
│                 │   │ (Implementation) │   │ (Future)     │
└─────────────────┘   └──────────────────┘   └──────────────┘
```

## Installation

This library is typically referenced by implementation libraries, not used directly.

If you need to use it directly:

```bash
dotnet add reference ../../Shared/EMIS.EventBus/EMIS.EventBus.csproj
```

## Core Abstractions

### IEvent

Base interface for all events:

```csharp
public interface IEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    string EventType { get; }
}
```

### IEventBus

Event bus interface for publishing and subscribing:

```csharp
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;
    
    void Subscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>;
    
    void Unsubscribe<TEvent, TEventHandler>()
        where TEvent : class, IEvent
        where TEventHandler : IEventHandler<TEvent>;
}
```

### IEventHandler

Handler interface for processing events:

```csharp
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
```

### BaseEvent

Abstract base class for events:

```csharp
public abstract class BaseEvent : IEvent
{
    public Guid EventId { get; init; }
    public DateTime Timestamp { get; init; }
    public abstract string EventType { get; }
}
```

## Domain Events

### TeacherCreatedEvent
**Topic**: `emis.auth.teacher.created`

Raised when admin creates a new teacher. AuthService consumes this to create authentication account.

**Properties**:
```csharp
public class TeacherCreatedEvent : BaseEvent
{
    public Guid TeacherId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Subject { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string DefaultPassword { get; set; }
    public Guid? SchoolId { get; set; }
}
```

### ParentCreatedEvent
**Topic**: `emis.auth.parent.created`

**Properties**:
```csharp
public class ParentCreatedEvent : BaseEvent
{
    public Guid ParentId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string DefaultPassword { get; set; }
    public List<Guid> StudentIds { get; set; }
}
```

### StudentCreatedEvent
**Topic**: `emis.auth.student.created`

**Properties**:
```csharp
public class StudentCreatedEvent : BaseEvent
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Grade { get; set; }
    public string? ClassName { get; set; }
    public string DefaultPassword { get; set; }
    public Guid? SchoolId { get; set; }
}
```

## Creating Custom Events

```csharp
using EMIS.EventBus.Events;

public class AssignmentCreatedEvent : BaseEvent
{
    public override string EventType => "emis.assignment.created";
    
    public Guid AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}
```

## Usage Pattern

### 1. Define Event (in this library)
```csharp
public class SomethingHappenedEvent : BaseEvent
{
    public override string EventType => "emis.domain.something.happened";
    // ... properties
}
```

### 2. Publish Event (in producer service)
```csharp
// Inject IEventBus (from implementation like EMIS.EventBus.Kafka)
public async Task DoSomething()
{
    var @event = new SomethingHappenedEvent { /* ... */ };
    await _eventBus.PublishAsync(@event);
}
```

### 3. Handle Event (in consumer service)
```csharp
public class SomethingHappenedEventHandler : IEventHandler<SomethingHappenedEvent>
{
    public async Task HandleAsync(SomethingHappenedEvent @event, CancellationToken ct)
    {
        // Process event
    }
}
```

## Implementation Libraries

See implementation-specific libraries for actual usage:

- **EMIS.EventBus.Kafka** - Apache Kafka implementation with:
  - 3-broker cluster
  - Idempotent producers
  - Manual offset management
  - Production-ready configuration
  - Full documentation: [../EMIS.EventBus.Kafka/README.md](../EMIS.EventBus.Kafka/README.md)

- **EMIS.EventBus.RabbitMQ** _(coming soon)_ - RabbitMQ implementation

## Design Principles

1. **Separation of Concerns**: Abstractions separate from implementations
2. **Dependency Inversion**: Depend on abstractions, not concrete implementations
3. **Open/Closed**: Open for extension (new events/handlers), closed for modification
4. **Interface Segregation**: Small, focused interfaces
5. **Liskov Substitution**: Implementations are interchangeable

## Dependencies

- **.NET 8.0** - No external dependencies (abstractions only)

## License

Internal EMIS project - not for public distribution.

## Features

- ✅ **Event Publishing**: Publish domain events to Kafka topics
- ✅ **Event Consumption**: Subscribe to events with type-safe handlers
- ✅ **Domain Events**: Pre-defined events for teacher, parent, and student account creation
- ✅ **High Availability**: 3-broker Kafka cluster with replication
- ✅ **Production Ready**: Idempotent producers, manual offset management, error handling
- ✅ **Easy Integration**: Simple DI registration with extension methods

## Architecture

```
┌─────────────────┐         ┌──────────────┐         ┌─────────────────┐
│ TeacherService  │  Pub    │  Kafka       │   Sub   │  AuthService    │
│                 ├────────▶│  Cluster     ├────────▶│                 │
│ Creates Teacher │         │ (3 brokers)  │         │ Creates Account │
└─────────────────┘         └──────────────┘         └─────────────────┘
```

## Installation

Add reference to your project:

```bash
dotnet add reference ../../Shared/EMIS.EventBus/EMIS.EventBus.csproj
```

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
    "EnableIdempotence": true
  }
}
```

### Configuration Options

| Property | Default | Description |
|----------|---------|-------------|
| `BootstrapServers` | `localhost:9092` | Comma-separated list of Kafka brokers |
| `GroupId` | `emis-event-bus` | Consumer group ID |
| `ClientId` | `emis-service` | Client identifier |
| `AutoOffsetReset` | `earliest` | Where to start consuming (earliest, latest, none) |
| `EnableAutoCommit` | `false` | Auto-commit offsets (false = manual control) |
| `Acks` | `all` | Producer acknowledgment level (0, 1, all) |
| `CompressionType` | `lz4` | Message compression (none, gzip, snappy, lz4, zstd) |
| `EnableIdempotence` | `true` | Exactly-once semantics |

## Usage

### 1. Publishing Events (Producer)

Register EventBus in `Program.cs`:

```csharp
using EMIS.EventBus.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add EventBus
builder.Services.AddKafkaEventBus(builder.Configuration);

var app = builder.Build();
```

Publish events in your service:

```csharp
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Events;

public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherResponse>
{
    private readonly IEventBus _eventBus;
    private readonly ITeacherRepository _teacherRepository;

    public CreateTeacherCommandHandler(IEventBus eventBus, ITeacherRepository teacherRepository)
    {
        _eventBus = eventBus;
        _teacherRepository = teacherRepository;
    }

    public async Task<TeacherResponse> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        // Create teacher in database
        var teacher = await _teacherRepository.CreateAsync(request);

        // Publish event for AuthService to create account
        var @event = new TeacherCreatedEvent
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            PhoneNumber = teacher.PhoneNumber,
            Subject = teacher.Subject,
            DefaultPassword = "Teacher@123" // Generate secure password
        };

        await _eventBus.PublishAsync(@event, cancellationToken);

        return MapToResponse(teacher);
    }
}
```

### 2. Consuming Events (Consumer)

Create an event handler:

```csharp
using EMIS.EventBus.Abstractions;
using EMIS.EventBus.Events;

public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeacherCreatedEventHandler> _logger;

    public TeacherCreatedEventHandler(IMediator mediator, ILogger<TeacherCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TeacherCreatedEvent: {TeacherId}", @event.TeacherId);

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

        await _mediator.Send(registerCommand, cancellationToken);

        _logger.LogInformation("Account created for teacher: {TeacherId}", @event.TeacherId);
    }
}
```

Register consumer in `Program.cs`:

```csharp
using EMIS.EventBus.Extensions;
using EMIS.EventBus.Events;

var builder = WebApplication.CreateBuilder(args);

// Add EventBus
builder.Services.AddKafkaEventBus(builder.Configuration);

// Register event handler
builder.Services.AddEventHandler<TeacherCreatedEvent, TeacherCreatedEventHandler>();

// Add consumer background service
builder.Services.AddKafkaConsumer(consumer =>
{
    // Register event types to consume
    consumer.RegisterEventType<TeacherCreatedEvent>("emis.auth.teacher.created");
    consumer.RegisterEventType<ParentCreatedEvent>("emis.auth.parent.created");
    consumer.RegisterEventType<StudentCreatedEvent>("emis.auth.student.created");
});

var app = builder.Build();
```

## Available Events

### TeacherCreatedEvent
Topic: `emis.auth.teacher.created`

Properties:
- `TeacherId` (Guid)
- `FullName` (string)
- `Email` (string)
- `PhoneNumber` (string?)
- `Subject` (string?)
- `DateOfBirth` (DateTime?)
- `DefaultPassword` (string)
- `SchoolId` (Guid?)

### ParentCreatedEvent
Topic: `emis.auth.parent.created`

Properties:
- `ParentId` (Guid)
- `FullName` (string)
- `Email` (string)
- `PhoneNumber` (string)
- `DateOfBirth` (DateTime?)
- `DefaultPassword` (string)
- `StudentIds` (List\<Guid\>)

### StudentCreatedEvent
Topic: `emis.auth.student.created`

Properties:
- `StudentId` (Guid)
- `FullName` (string)
- `Email` (string?)
- `PhoneNumber` (string?)
- `DateOfBirth` (DateTime)
- `Grade` (string?)
- `ClassName` (string?)
- `DefaultPassword` (string)
- `SchoolId` (Guid?)

## Creating Custom Events

```csharp
using EMIS.EventBus.Events;

public class CustomEvent : BaseEvent
{
    public override string EventType => "emis.custom.event";

    public string CustomProperty { get; set; } = string.Empty;
}
```

## Topic Management

### Creating Topics

Use Kafka UI (http://localhost:8080) or CLI:

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
```

### Recommended Settings

For Vietnam-wide scale (25,000 schools, 25M users):

```bash
# Teacher creation events (10K+ teachers/day)
Partitions: 10
Replication: 3
Min ISR: 2
Retention: 7 days

# Parent creation events (20K+ parents/day)
Partitions: 15
Replication: 3
Min ISR: 2
Retention: 7 days

# Student creation events (100K+ students/year, peaks during enrollment)
Partitions: 20
Replication: 3
Min ISR: 2
Retention: 7 days
```

## Error Handling

### Automatic Retries

Producer retries are configured automatically:
- `MessageSendMaxRetries`: 3
- Exponential backoff

### Dead Letter Queue (DLQ)

For events that fail after retries, implement DLQ pattern:

```csharp
public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken cancellationToken)
{
    try
    {
        await ProcessEventAsync(@event, cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process event {EventId}", @event.EventId);
        
        // Publish to DLQ topic
        await _eventBus.PublishAsync(new DeadLetterEvent
        {
            OriginalEvent = @event,
            Error = ex.Message,
            RetryCount = 3
        }, cancellationToken);
        
        // Don't throw - allow consumer to commit offset
    }
}
```

## Monitoring

### Kafka UI

Access: http://localhost:8080

Features:
- View topics and partitions
- Monitor consumer groups
- Inspect messages
- View broker metrics

### Logging

EventBus logs important events:
- Event published (topic, partition, offset)
- Event consumed (topic, partition, offset)
- Handler execution
- Errors and warnings

## Production Considerations

1. **Security**: Enable SASL/SSL in production
2. **Monitoring**: Use Prometheus + Grafana for metrics
3. **Alerting**: Set alerts for consumer lag
4. **Retention**: Adjust retention based on storage
5. **Partitioning**: More partitions = higher throughput
6. **Consumer Groups**: Scale consumers horizontally

## Dependencies

- Confluent.Kafka 2.12.0
- Microsoft.Extensions.DependencyInjection.Abstractions 9.0.10
- Microsoft.Extensions.Logging.Abstractions 9.0.10
- Microsoft.Extensions.Configuration.Abstractions 9.0.10
- Microsoft.Extensions.Configuration.Binder 9.0.10
- Microsoft.Extensions.Hosting.Abstractions 9.0.10

## License

Internal EMIS project - not for public distribution.
