# EMIS.Contracts

**Shared contracts library** for EMIS microservices - integration events, constants, enums, and future DTOs.

> 📜 **This library contains the "contracts" (agreements) between services** - events, shared types, constants that all services must understand consistently.

## Purpose

In a microservices architecture, services need to communicate. `EMIS.Contracts` defines the **common language** they use:

- 📨 **Integration Events** - What events are published and consumed
- 🏷️ **Enums** - Shared enumerations (UserRole, StudentStatus, etc.)
- 🔑 **Constants** - Topic names, error codes, cache keys
- 📋 **DTOs** _(future)_ - Data Transfer Objects for sync APIs

## Architecture

```
┌────────────────────────────────────────────────────────┐
│                   EMIS.Contracts                        │
│  (Shared contracts - the "API surface" between services)│
└───────────────────────┬────────────────────────────────┘
                        │
        ┌───────────────┼───────────────┬────────────────┐
        │               │               │                │
┌───────▼────────┐ ┌────▼─────────┐ ┌──▼──────────┐ ┌───▼──────────┐
│ TeacherService │ │ AuthService  │ │ StudentSvc  │ │ NotifySvc    │
│  (Publisher)   │ │  (Consumer)  │ │ (Publisher) │ │  (Consumer)  │
└────────────────┘ └──────────────┘ └─────────────┘ └──────────────┘
```

## Contents

### 📁 Events/ - Integration Events

Domain events for asynchronous communication via Kafka:

```
Events/
├── BaseEvent.cs                 # Abstract base class
├── TeacherCreatedEvent.cs       # Teacher account creation
├── ParentCreatedEvent.cs        # Parent account creation
└── StudentCreatedEvent.cs       # Student account creation
```

**Usage**:
```csharp
using EMIS.Contracts.Events;

// Publish event
var @event = new TeacherCreatedEvent
{
    TeacherId = teacher.Id,
    FullName = teacher.FullName,
    Email = teacher.Email,
    DefaultPassword = generatedPassword
};
await _eventBus.PublishAsync(@event);
```

**Topic naming**: See `Constants/TopicNames.cs` for standard topic names.

---

### 📁 Constants/ - Shared Constants

#### TopicNames.cs
Kafka topic names organized by domain:

```csharp
using EMIS.Contracts.Constants;

// Auth topics
TopicNames.Auth.TeacherCreated    // "emis.auth.teacher.created"
TopicNames.Auth.UserPasswordChanged

// Student topics
TopicNames.Student.Enrolled       // "emis.student.enrolled"
TopicNames.Student.Graduated

// Notification topics
TopicNames.Notification.EmailQueued
```

#### ErrorCodes.cs
Standardized error codes for consistent error handling:

```csharp
using EMIS.Contracts.Constants;

// Return consistent error codes
throw new NotFoundException(ErrorCodes.Auth.UserNotFound);
throw new ValidationException(ErrorCodes.Validation.InvalidEmail);
```

**Categories**:
- `ErrorCodes.Auth.*` - Authentication errors (AUTH_001, AUTH_002...)
- `ErrorCodes.Student.*` - Student management (STU_001...)
- `ErrorCodes.Teacher.*` - Teacher management (TCH_001...)
- `ErrorCodes.Validation.*` - Validation errors (VAL_001...)
- `ErrorCodes.System.*` - System errors (SYS_001...)

#### CacheKeys.cs
Redis cache key formatters:

```csharp
using EMIS.Contracts.Constants;

// Generate cache keys
var key = CacheKeys.Auth.UserInfo(userId);
// Returns: "auth:user:info:{userId}"

var attendanceKey = CacheKeys.Attendance.DailyRecord(classId, date);
// Returns: "attendance:daily:{classId}:{date}"
```

**Expiration times**:
```csharp
CacheKeys.Expiration.Short    // 5 minutes
CacheKeys.Expiration.Medium   // 30 minutes
CacheKeys.Expiration.Long     // 2 hours
CacheKeys.Expiration.VeryLong // 24 hours
```

---

### 📁 Enums/ - Shared Enumerations

#### UserRole.cs
```csharp
public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Parent = 3,
    Student = 4,
    SchoolAdmin = 5,
    AcademicStaff = 6
}
```

#### StudentStatus.cs
```csharp
public enum StudentStatus
{
    Active = 1,
    Inactive = 2,
    Transferred = 3,
    Graduated = 4,
    Dropped = 5,
    Suspended = 6,
    Expelled = 7,
    Pending = 8
}
```

#### AttendanceStatus.cs
```csharp
public enum AttendanceStatus
{
    NotMarked = 0,
    Present = 1,
    Absent = 2,
    Late = 3,
    Excused = 4,
    LeftEarly = 5
}
```

#### GradeLevel.cs
Vietnamese education system grades:

```csharp
public enum GradeLevel
{
    Kindergarten = 0,
    Grade1 = 1,
    Grade2 = 2,
    // ...
    Grade12 = 12
}
```

#### NotificationEnums.cs
```csharp
public enum NotificationChannel
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4,
    All = 99
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}
```

---

## Installation

Add reference to your service project:

```bash
# From TeacherService/Teacher.Application
dotnet add reference ../../../Shared/EMIS.Contracts/EMIS.Contracts.csproj
```

This automatically includes `EMIS.EventBus` as a transitive dependency.

---

## Usage Patterns

### 1. Publishing Events (Producer)

```csharp
using EMIS.Contracts.Events;
using EMIS.Contracts.Constants;
using EMIS.EventBus.Abstractions;

public class CreateTeacherHandler
{
    private readonly IEventBus _eventBus;

    public async Task Handle(CreateTeacherCommand command)
    {
        // Create teacher in database
        var teacher = await _repository.CreateAsync(command);

        // Publish event
        var @event = new TeacherCreatedEvent
        {
            TeacherId = teacher.Id,
            FullName = teacher.FullName,
            Email = teacher.Email,
            PhoneNumber = teacher.PhoneNumber,
            Subject = teacher.Subject,
            DefaultPassword = GeneratePassword()
        };

        await _eventBus.PublishAsync(@event);
        
        _logger.LogInformation(
            "Published {EventType} to {Topic}", 
            @event.EventType, 
            TopicNames.Auth.TeacherCreated);
    }
}
```

### 2. Handling Events (Consumer)

```csharp
using EMIS.Contracts.Events;
using EMIS.Contracts.Enums;
using EMIS.EventBus.Abstractions;

public class TeacherCreatedEventHandler : IEventHandler<TeacherCreatedEvent>
{
    public async Task HandleAsync(TeacherCreatedEvent @event, CancellationToken ct)
    {
        // Create authentication account
        var user = new User
        {
            Username = @event.Email,
            Email = @event.Email,
            FullName = @event.FullName,
            Roles = new List<string> { UserRole.Teacher.ToString() }
        };

        await _userRepository.CreateAsync(user);
    }
}
```

### 3. Using Constants

```csharp
using EMIS.Contracts.Constants;

// Error handling
if (user == null)
    throw new NotFoundException(ErrorCodes.Auth.UserNotFound);

// Cache keys
var cacheKey = CacheKeys.Auth.UserInfo(userId);
var cachedUser = await _cache.GetAsync<User>(cacheKey);

if (cachedUser == null)
{
    cachedUser = await _repository.GetByIdAsync(userId);
    await _cache.SetAsync(cacheKey, cachedUser, CacheKeys.Expiration.Medium);
}

// Topic names in consumer registration
consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Auth.TeacherCreated);
```

### 4. Using Enums

```csharp
using EMIS.Contracts.Enums;

// Type-safe enums
public class Student
{
    public StudentStatus Status { get; set; } = StudentStatus.Pending;
    public GradeLevel Grade { get; set; }
}

// Check roles
if (user.Role == UserRole.Teacher)
{
    // Teacher-specific logic
}

// Attendance marking
var attendance = new Attendance
{
    Status = AttendanceStatus.Present,
    MarkedAt = DateTime.UtcNow
};
```

---

## Best Practices

### ✅ DO

1. **Use constants for topic names**
   ```csharp
   // ✅ Good
   consumer.RegisterEventType<TeacherCreatedEvent>(TopicNames.Auth.TeacherCreated);
   
   // ❌ Bad - hardcoded string
   consumer.RegisterEventType<TeacherCreatedEvent>("emis.auth.teacher.created");
   ```

2. **Use enums for shared types**
   ```csharp
   // ✅ Good - type safe
   if (student.Status == StudentStatus.Active)
   
   // ❌ Bad - magic strings
   if (student.Status == "Active")
   ```

3. **Use error codes for standardization**
   ```csharp
   // ✅ Good
   return Problem(ErrorCodes.Auth.InvalidCredentials, "Invalid username or password");
   
   // ❌ Bad
   return Problem("LOGIN_FAILED", "Bad credentials");
   ```

### ⚠️ Versioning

When making breaking changes to events:

**Option 1: Version the event**
```csharp
// Old version still supported
public class TeacherCreatedEventV1 : BaseEvent { ... }

// New version with breaking changes
public class TeacherCreatedEventV2 : BaseEvent 
{
    // New required field
    public Guid SchoolId { get; set; }
}
```

**Option 2: Make fields optional**
```csharp
public class TeacherCreatedEvent : BaseEvent
{
    // Existing fields
    public string Email { get; set; }
    
    // New field - nullable (backward compatible)
    public Guid? SchoolId { get; set; }
}
```

### 🚫 DON'T

1. **Don't put business logic in Contracts**
   - This is for data structures and constants only
   - Business logic belongs in individual services

2. **Don't share database entities**
   - Each service has its own domain models
   - Use events/DTOs for communication

3. **Don't make breaking changes without versioning**
   - Removing fields → breaking change
   - Changing field types → breaking change
   - Adding optional fields → safe

---

## Future Additions

### DTOs (Coming Soon)

For synchronous HTTP communication:

```csharp
// EMIS.Contracts/DTOs/Auth/
public class ValidateTokenRequest
{
    public string AccessToken { get; set; }
}

public class UserInfoDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public List<UserRole> Roles { get; set; }
}
```

### Value Objects (Coming Soon)

Domain-driven design value objects:

```csharp
// EMIS.Contracts/ValueObjects/
public class Email : ValueObject
{
    public string Value { get; }
    // Validation logic
}

public class PhoneNumber : ValueObject
{
    public string Value { get; }
    // Validation logic
}
```

---

## Dependencies

- **EMIS.EventBus** (project reference) - For `BaseEvent` and `IEvent`
- **.NET 8.0** - No other external dependencies

## Version

**Current**: 1.0.0

### Versioning Strategy
- **Major** (x.0.0): Breaking changes (remove fields, change types)
- **Minor** (1.x.0): New features (add optional fields, new events)
- **Patch** (1.0.x): Bug fixes, documentation

---

## Contributing

When adding new contracts:

1. **Events**: Add to `Events/` folder
2. **Enums**: Add to `Enums/` folder
3. **Constants**: Add to appropriate class in `Constants/`
4. **Update README**: Document new additions
5. **Bump version**: Follow semantic versioning

---

## License

Internal EMIS project - not for public distribution.
