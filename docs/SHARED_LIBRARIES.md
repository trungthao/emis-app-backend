# Shared Libraries Overview

Hệ thống EMIS có 4 shared libraries chính, mỗi library có trách nhiệm riêng biệt theo Clean Architecture principles.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Service Layer                            │
│  (TeacherService, StudentService, AuthService, etc.)            │
└───────────────────────┬─────────────────────────────────────────┘
                        │ depends on
        ┌───────────────┼───────────────┬───────────────┐
        │               │               │               │
        ▼               ▼               ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│EMIS.EventBus │ │EMIS.Contracts│ │EMIS.EventBus │ │EMIS.         │
│              │ │              │ │.Kafka        │ │Authentication│
│ Abstractions │ │ Events, DTOs │ │ Kafka Impl   │ │ JWT Utils    │
└──────┬───────┘ └──────┬───────┘ └──────┬───────┘ └──────────────┘
       │                │                │
       │                ▼                │
       │         ┌──────────────┐        │
       └────────▶│ EMIS.EventBus│◀───────┘
                 │ (referenced) │
                 └──────────────┘
```

---

## 1. EMIS.EventBus

**📍 Location**: `src/Shared/EMIS.EventBus/`

**🎯 Purpose**: Core abstractions for event-driven architecture

**📦 Contents**:
```
EMIS.EventBus/
├── Abstractions/
│   ├── IEvent.cs              # Base event interface
│   ├── IEventBus.cs           # Publish/Subscribe interface
│   └── IEventHandler.cs       # Event handler interface
└── README.md
```

**🔗 Dependencies**: 
- ✅ None (pure abstractions)

**👥 Used by**:
- EMIS.Contracts (inherits from `IEvent`)
- EMIS.EventBus.Kafka (implements `IEventBus`)
- All services (depend on `IEventBus` interface)

**📝 Example**:
```csharp
// Define what an event bus should do
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IEvent;
}
```

---

## 2. EMIS.Contracts

**📍 Location**: `src/Shared/EMIS.Contracts/`

**🎯 Purpose**: Shared contracts (events, enums, constants) between microservices

**📦 Contents**:
```
EMIS.Contracts/
├── Events/                    # Integration events
│   ├── BaseEvent.cs
│   ├── TeacherCreatedEvent.cs
│   ├── ParentCreatedEvent.cs
│   └── StudentCreatedEvent.cs
├── Constants/                 # Shared constants
│   ├── TopicNames.cs         # Kafka topic names
│   ├── ErrorCodes.cs         # Standardized error codes
│   └── CacheKeys.cs          # Redis cache key formatters
├── Enums/                     # Shared enumerations
│   ├── UserRole.cs
│   ├── StudentStatus.cs
│   ├── AttendanceStatus.cs
│   ├── GradeLevel.cs
│   └── NotificationEnums.cs
└── README.md
```

**🔗 Dependencies**:
- EMIS.EventBus (for `IEvent` and `BaseEvent`)

**👥 Used by**:
- All services that publish or consume events
- Services that need shared enums/constants

**📝 Example**:
```csharp
using EMIS.Contracts.Events;
using EMIS.Contracts.Constants;
using EMIS.Contracts.Enums;

// Use domain event
var @event = new TeacherCreatedEvent { ... };

// Use constants
consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Auth.TeacherCreated);

// Use enums
if (user.Role == UserRole.Teacher) { ... }
```

---

## 3. EMIS.EventBus.Kafka

**📍 Location**: `src/Shared/EMIS.EventBus.Kafka/`

**🎯 Purpose**: Apache Kafka implementation of EventBus abstractions

**📦 Contents**:
```
EMIS.EventBus.Kafka/
├── Configuration/
│   └── KafkaConfiguration.cs   # Kafka settings
├── Kafka/
│   ├── KafkaEventBus.cs        # IEventBus implementation
│   └── KafkaConsumerService.cs # Background consumer
├── Extensions/
│   └── KafkaEventBusExtensions.cs # DI registration
└── README.md
```

**🔗 Dependencies**:
- EMIS.EventBus (implements interfaces)
- Confluent.Kafka (Kafka client library)
- Microsoft.Extensions.* (DI, Logging, Configuration, Hosting)

**👥 Used by**:
- Services that need to publish events (producers)
- Services that need to consume events (consumers)

**📝 Example**:
```csharp
// In Program.cs
using EMIS.EventBus.Kafka.Extensions;

builder.Services.AddKafkaEventBus(builder.Configuration);

// Configuration in appsettings.json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092,localhost:9093,localhost:9094",
    "GroupId": "emis-auth-service"
  }
}
```

---

## 4. EMIS.Authentication

**📍 Location**: `src/Shared/EMIS.Authentication/`

**🎯 Purpose**: JWT authentication/authorization utilities

**📦 Contents**:
```
EMIS.Authentication/
├── Extensions/
│   ├── AuthenticationExtensions.cs
│   └── AuthorizationExtensions.cs
├── Models/
│   └── JwtSettings.cs
└── Services/
    └── TokenService.cs
```

**🔗 Dependencies**:
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt

**👥 Used by**:
- All services that need JWT authentication
- AuthService (generates tokens)
- Other services (validate tokens)

**📝 Example**:
```csharp
// In Program.cs
using EMIS.Authentication.Extensions;

builder.Services.AddEmisAuthentication(builder.Configuration);
builder.Services.AddEmisAuthorization();

// In controller
[Authorize(Roles = "Teacher")]
public class TeachersController : ControllerBase { ... }
```

---

## Dependency Graph

```
┌─────────────────┐
│ EMIS.EventBus   │ ← No dependencies (pure abstractions)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ EMIS.Contracts  │ ← Depends on: EMIS.EventBus
└────────┬────────┘
         │
         ▼
┌───────────────────┐
│EMIS.EventBus.Kafka│ ← Depends on: EMIS.EventBus, Confluent.Kafka
└───────────────────┘

┌────────────────────┐
│EMIS.Authentication │ ← Depends on: JWT libraries (independent)
└────────────────────┘

                ┌──────────────────────┐
                │  TeacherService      │
                │  ──────────────      │
                │  References:         │
                │  - EMIS.Contracts    │ (gets EventBus transitively)
                │  - EMIS.EventBus     │
                │    .Kafka            │ (gets Contracts, EventBus)
                │  - EMIS.             │
                │    Authentication    │
                └──────────────────────┘
```

---

## When to Use Each Library

### Use EMIS.EventBus when:
- ❌ **Never directly** (it's just abstractions)
- ✅ Referenced transitively through Contracts or EventBus.Kafka

### Use EMIS.Contracts when:
- ✅ Publishing events (need event classes)
- ✅ Consuming events (need event classes)
- ✅ Using shared enums (UserRole, StudentStatus, etc.)
- ✅ Using constants (topic names, error codes, cache keys)

### Use EMIS.EventBus.Kafka when:
- ✅ Service needs to publish events to Kafka
- ✅ Service needs to consume events from Kafka
- ✅ Implementing event-driven communication

### Use EMIS.Authentication when:
- ✅ Service needs JWT authentication
- ✅ Service needs to validate tokens
- ✅ AuthService (generates tokens)

---

## Reference Pattern for Services

### Producer Service (e.g., TeacherService)

```xml
<!-- Teacher.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="../../../Shared/EMIS.Contracts/EMIS.Contracts.csproj" />
  <ProjectReference Include="../../../Shared/EMIS.EventBus.Kafka/EMIS.EventBus.Kafka.csproj" />
</ItemGroup>
```

```csharp
// Teacher.API/Program.cs
using EMIS.EventBus.Kafka.Extensions;

builder.Services.AddKafkaEventBus(builder.Configuration);

// CreateTeacherHandler.cs
using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;

public class CreateTeacherHandler
{
    private readonly IEventBus _eventBus; // ← From EMIS.EventBus (abstraction)
    
    public async Task Handle(...)
    {
        var @event = new TeacherCreatedEvent { ... }; // ← From EMIS.Contracts
        await _eventBus.PublishAsync(@event);
    }
}
```

### Consumer Service (e.g., AuthService)

```xml
<!-- Auth.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="../../../Shared/EMIS.Contracts/EMIS.Contracts.csproj" />
  <ProjectReference Include="../../../Shared/EMIS.EventBus.Kafka/EMIS.EventBus.Kafka.csproj" />
</ItemGroup>
```

```csharp
// Auth.API/Program.cs
using EMIS.EventBus.Kafka.Extensions;
using EMIS.Contracts.Events;
using EMIS.Contracts.Constants;

builder.Services.AddKafkaEventBus(builder.Configuration);
builder.Services.AddEventHandler<TeacherCreatedEvent, TeacherCreatedEventHandler>();
builder.Services.AddKafkaConsumer(consumer =>
{
    consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Auth.TeacherCreated);
});

// TeacherCreatedEventHandler.cs
using EMIS.Contracts.Events;
using EMIS.EventBus.Abstractions;

public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
{
    public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken ct)
    {
        // Create user account
    }
}
```

---

## Version Management

| Library | Version | Breaking Change Strategy |
|---------|---------|--------------------------|
| EMIS.EventBus | 1.0.0 | Major bump if interface changes |
| EMIS.Contracts | 1.0.0 | Major bump if events/enums change incompatibly |
| EMIS.EventBus.Kafka | 1.0.0 | Major bump if configuration changes |
| EMIS.Authentication | 1.0.0 | Major bump if JWT format changes |

---

## Best Practices

### ✅ DO

1. **Reference only what you need**
   ```csharp
   // If you only publish events, reference:
   // - EMIS.Contracts (events)
   // - EMIS.EventBus.Kafka (implementation)
   ```

2. **Use constants from EMIS.Contracts**
   ```csharp
   // ✅ Good
   consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Auth.TeacherCreated);
   
   // ❌ Bad
   consumer.RegisterEventType<TeacherCreatedEvent>("emis.auth.teacher.created");
   ```

3. **Depend on abstractions in business logic**
   ```csharp
   // ✅ Good - depends on IEventBus
   public class MyHandler
   {
       private readonly IEventBus _eventBus;
   }
   
   // ❌ Bad - depends on KafkaEventBus
   public class MyHandler
   {
       private readonly KafkaEventBus _kafkaEventBus;
   }
   ```

### ❌ DON'T

1. **Don't put business logic in shared libraries**
   - Shared libraries are for contracts and infrastructure only
   - Business logic belongs in services

2. **Don't create circular dependencies**
   - Services → Contracts ✅
   - Contracts → Services ❌

3. **Don't reference EMIS.EventBus directly**
   - Reference EMIS.Contracts or EMIS.EventBus.Kafka instead
   - EMIS.EventBus will be included transitively

---

## Migration from Old Structure

### Before (Old Structure)
```
EMIS.EventBus/
├── Abstractions/
├── Events/           ← Events were here
├── Kafka/            ← Implementation was here
├── Configuration/
└── Extensions/
```

### After (New Structure)
```
EMIS.EventBus/        ← Only abstractions
└── Abstractions/

EMIS.Contracts/       ← Events moved here
├── Events/
├── Constants/
└── Enums/

EMIS.EventBus.Kafka/  ← Implementation moved here
├── Kafka/
├── Configuration/
└── Extensions/
```

---

## Summary

| Library | Purpose | Dependencies | Used For |
|---------|---------|--------------|----------|
| **EMIS.EventBus** | Abstractions | None | Interfaces only |
| **EMIS.Contracts** | Shared contracts | EventBus | Events, Enums, Constants |
| **EMIS.EventBus.Kafka** | Kafka implementation | EventBus, Confluent.Kafka | Publish/consume via Kafka |
| **EMIS.Authentication** | JWT auth | JWT libraries | Authentication |

**Recommended reference pattern**:
```
Service → EMIS.Contracts + EMIS.EventBus.Kafka + EMIS.Authentication
```

This brings in all necessary libraries with proper dependency hierarchy.
