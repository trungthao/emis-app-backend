# EventBus Architecture Restructure

## Overview

The EventBus has been restructured to follow **separation of concerns** and **dependency inversion** principles, making it extensible for multiple message broker implementations.

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                     EMIS.EventBus                             │
│                  (Abstractions Only)                          │
│                                                               │
│  • IEvent, IEventBus, IEventHandler                          │
│  • BaseEvent                                                  │
│  • Domain Events (TeacherCreated, ParentCreated, etc.)       │
│                                                               │
└───────────────────────────┬──────────────────────────────────┘
                            │
                            │ implements
            ┌───────────────┴───────────────┬──────────────────┐
            │                               │                  │
            ▼                               ▼                  ▼
┌───────────────────────┐       ┌───────────────────┐   ┌──────────────┐
│ EMIS.EventBus.Kafka   │       │ EMIS.EventBus     │   │ Future:      │
│                       │       │ .RabbitMQ         │   │ .AzureServiceBus │
│ • KafkaEventBus       │       │                   │   │ .GooglePubSub│
│ • KafkaConsumer       │       │ (Coming Soon)     │   └──────────────┘
│ • KafkaConfiguration  │       │                   │
│ • Extensions          │       └───────────────────┘
└───────────────────────┘
```

## Project Structure

### EMIS.EventBus (Core Abstractions)

**Location**: `src/Shared/EMIS.EventBus/`

**Purpose**: Contains only interfaces, base classes, and domain events. No implementation details.

**Contents**:
```
EMIS.EventBus/
├── Abstractions/
│   ├── IEvent.cs              # Base event interface
│   ├── IEventBus.cs           # Publisher/Subscriber interface
│   └── IEventHandler.cs       # Event handler interface
├── Events/
│   ├── BaseEvent.cs           # Abstract base event
│   ├── TeacherCreatedEvent.cs # Domain event
│   ├── ParentCreatedEvent.cs  # Domain event
│   └── StudentCreatedEvent.cs # Domain event
├── README.md
└── EMIS.EventBus.csproj
```

**Dependencies**: 
- ✅ None (pure abstractions)

**When to modify**:
- Adding new domain events
- Changing event interfaces
- Adding new event properties

### EMIS.EventBus.Kafka (Kafka Implementation)

**Location**: `src/Shared/EMIS.EventBus.Kafka/`

**Purpose**: Apache Kafka implementation of EventBus abstractions. Production-ready with Confluent Kafka.

**Contents**:
```
EMIS.EventBus.Kafka/
├── Configuration/
│   └── KafkaConfiguration.cs   # Kafka settings (brokers, compression, etc.)
├── Kafka/
│   ├── KafkaEventBus.cs        # IEventBus implementation (producer)
│   └── KafkaConsumerService.cs # Background consumer service
├── Extensions/
│   └── KafkaEventBusExtensions.cs # DI registration
├── README.md
└── EMIS.EventBus.Kafka.csproj
```

**Dependencies**:
- ✅ EMIS.EventBus (project reference)
- ✅ Confluent.Kafka 2.12.0
- ✅ Microsoft.Extensions.* (DI, Logging, Configuration, Hosting)

**When to modify**:
- Kafka-specific configurations
- Producer/Consumer logic
- Error handling strategies
- Performance tuning

## Usage Patterns

### For Service Developers

#### Producer (Publishing Events)

```csharp
// In TeacherService/Program.cs
using EMIS.EventBus.Kafka.Extensions; // ← Use Kafka implementation

builder.Services.AddKafkaEventBus(builder.Configuration);

// In CreateTeacherCommandHandler
using EMIS.EventBus.Abstractions;     // ← Depend on abstraction
using EMIS.EventBus.Events;           // ← Use domain events

public class CreateTeacherCommandHandler
{
    private readonly IEventBus _eventBus; // ← Interface, not concrete class
    
    public async Task Handle(...)
    {
        var @event = new TeacherCreatedEvent { ... };
        await _eventBus.PublishAsync(@event);
    }
}
```

#### Consumer (Handling Events)

```csharp
// In AuthService/Program.cs
using EMIS.EventBus.Kafka.Extensions;
using EMIS.EventBus.Events;

builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddEventHandler<TeacherCreatedEvent, TeacherCreatedEventHandler>();
builder.Services.AddKafkaConsumer(consumer =>
{
    consumer.RegisterEventType<TeacherCreatedEvent>("emis.auth.teacher.created");
});

// In TeacherCreatedEventHandler
using EMIS.EventBus.Abstractions;

public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
{
    public async Task HandleAsync(TeacherCreatedEvent @event, ...)
    {
        // Create account
    }
}
```

## Benefits of This Architecture

### 1. **Separation of Concerns**
- Abstractions (contracts) separated from implementations
- Domain events independent of transport mechanism
- Clean dependency graph

### 2. **Dependency Inversion**
- Services depend on `IEventBus`, not `KafkaEventBus`
- Easy to mock for unit testing
- Loosely coupled architecture

### 3. **Open for Extension**
- Add new implementations (RabbitMQ, Azure Service Bus) without changing existing code
- Add new events without modifying EventBus
- Extensible handler registration

### 4. **Testability**
```csharp
// Easy to mock
var mockEventBus = new Mock<IEventBus>();
mockEventBus.Setup(x => x.PublishAsync(It.IsAny<TeacherCreatedEvent>(), ...))
            .ReturnsAsync();
```

### 5. **Flexibility**
- Switch from Kafka to RabbitMQ by changing one line:
  ```csharp
  // Before: builder.Services.AddKafkaEventBus(...)
  // After:  builder.Services.AddRabbitMQEventBus(...)
  ```

## Future Implementations

### EMIS.EventBus.RabbitMQ

For scenarios where RabbitMQ is preferred:
- Smaller deployments
- Simpler operational model
- RPC-style messaging patterns

**Structure** (planned):
```
EMIS.EventBus.RabbitMQ/
├── Configuration/
│   └── RabbitMQConfiguration.cs
├── RabbitMQ/
│   ├── RabbitMQEventBus.cs
│   └── RabbitMQConsumerService.cs
└── Extensions/
    └── RabbitMQEventBusExtensions.cs
```

### EMIS.EventBus.AzureServiceBus

For Azure cloud deployments:
- Enterprise messaging
- Cloud-native integration
- Managed service

## Migration Guide

### Existing Code Migration

If you have existing code using the old structure, update:

**Before**:
```csharp
using EMIS.EventBus.Extensions;  // ❌ Old
using EMIS.EventBus.Kafka;       // ❌ Old

builder.Services.AddKafkaEventBus(...);  // ❌ No longer exists
```

**After**:
```csharp
using EMIS.EventBus.Kafka.Extensions;  // ✅ New
using EMIS.EventBus.Abstractions;      // ✅ New

builder.Services.AddKafkaEventBus(...);  // ✅ From Kafka implementation
```

### Project References

**Before**:
```xml
<ProjectReference Include="../../Shared/EMIS.EventBus/EMIS.EventBus.csproj" />
```

**After**:
```xml
<!-- Only reference the implementation, it will bring in abstractions -->
<ProjectReference Include="../../Shared/EMIS.EventBus.Kafka/EMIS.EventBus.Kafka.csproj" />
```

## Design Principles Applied

1. ✅ **Single Responsibility**: Each project has one reason to change
2. ✅ **Open/Closed**: Open for extension (new implementations), closed for modification
3. ✅ **Liskov Substitution**: Any IEventBus implementation can be swapped
4. ✅ **Interface Segregation**: Small, focused interfaces
5. ✅ **Dependency Inversion**: Depend on abstractions, not concretions

## FAQs

### Q: Which project should I reference in my service?

**A**: Reference `EMIS.EventBus.Kafka` (or whichever implementation you use). It will automatically include `EMIS.EventBus` as a transitive dependency.

### Q: Can I use multiple implementations?

**A**: Yes, but typically you'd use one per environment:
- Development: Kafka (for similarity to production)
- Production: Kafka (high throughput)
- Testing: In-memory implementation (fast, no dependencies)

### Q: Where do I add new domain events?

**A**: Add to `EMIS.EventBus/Events/` - they belong to the domain, not the transport mechanism.

### Q: How do I switch from Kafka to RabbitMQ?

**A**: 
1. Reference `EMIS.EventBus.RabbitMQ` instead of `EMIS.EventBus.Kafka`
2. Change extension method: `AddRabbitMQEventBus()` instead of `AddKafkaEventBus()`
3. Update `appsettings.json` with RabbitMQ configuration

### Q: Can I create a custom implementation?

**A**: Yes! Implement `IEventBus` and create your own extension methods. Follow the same pattern as `EMIS.EventBus.Kafka`.

## Version History

- **v1.0.0** - Initial monolithic structure (Kafka implementation in EMIS.EventBus)
- **v2.0.0** - **Current**: Restructured into abstractions + implementations
  - Created `EMIS.EventBus` (abstractions only)
  - Created `EMIS.EventBus.Kafka` (Kafka implementation)
  - Removed Kafka dependencies from core EventBus

## Next Steps

1. ✅ **Done**: Restructure EventBus architecture
2. 🔄 **In Progress**: Integrate into TeacherService (producer)
3. ⏳ **Pending**: Integrate into AuthService (consumer)
4. ⏳ **Pending**: Create Kafka topics
5. ⏳ **Pending**: Test end-to-end flow
6. 🔮 **Future**: Implement EMIS.EventBus.RabbitMQ

## References

- [EMIS.EventBus README](../EMIS.EventBus/README.md)
- [EMIS.EventBus.Kafka README](../EMIS.EventBus.Kafka/README.md)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
