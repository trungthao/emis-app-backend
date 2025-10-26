using EMIS.EventBus.Abstractions;

namespace EMIS.Contracts.Examples;

/// <summary>
/// Example: Hybrid Topic Strategy - Topic per Aggregate
/// This is an ALTERNATIVE approach if you want to consolidate topics in the future
/// </summary>

#region Event Base Classes

/// <summary>
/// Base event for aggregate topics
/// All events in the same aggregate topic inherit from this
/// </summary>
public abstract class AggregateEvent : IEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event type discriminator - used to distinguish events in same topic
    /// Examples: "student.created", "student.updated", "student.enrolled"
    /// </summary>
    public abstract string EventType { get; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Aggregate ID - used as partition key to ensure ordering
    /// </summary>
    public abstract Guid GetAggregateId();
}

#endregion

#region Student Aggregate Events

/// <summary>
/// All student events go to same topic: "emis.student"
/// Partitioned by StudentId to ensure ordering
/// </summary>
public class StudentCreatedEvent : AggregateEvent
{
    public override string EventType => "student.created";

    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Guid? SchoolId { get; set; }

    public override Guid GetAggregateId() => StudentId;
}

public class StudentUpdatedEvent : AggregateEvent
{
    public override string EventType => "student.updated";

    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }

    public override Guid GetAggregateId() => StudentId;
}

public class StudentEnrolledEvent : AggregateEvent
{
    public override string EventType => "student.enrolled";

    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public override Guid GetAggregateId() => StudentId;
}

/// <summary>
/// Critical event - might have separate topic: "emis.student.graduated"
/// </summary>
public class StudentGraduatedEvent : AggregateEvent
{
    public override string EventType => "student.graduated";

    public Guid StudentId { get; set; }
    public DateTime GraduationDate { get; set; }
    public string? Honors { get; set; }

    public override Guid GetAggregateId() => StudentId;
}

#endregion

#region Topic Names - Hybrid Strategy

public static class TopicNamesHybrid
{
    /// <summary>
    /// TIER 1: Core Entity Events (Topic per Aggregate)
    /// </summary>
    public static class Student
    {
        // Main aggregate topic - all student events except critical ones
        public const string Events = "emis.student";

        // Critical event - separate topic for special handling
        public const string Graduated = "emis.student.graduated";
    }

    public static class Teacher
    {
        public const string Events = "emis.teacher";
    }

    public static class Class
    {
        public const string Events = "emis.class";
    }

    /// <summary>
    /// TIER 2: High-Volume Events (Separate Topics)
    /// </summary>
    public static class Attendance
    {
        // Separate topics due to high volume (millions/day)
        public const string Marked = "emis.attendance.marked";
        public const string AbsenceReported = "emis.attendance.absence.reported";
    }

    /// <summary>
    /// TIER 3: Critical Business Events (Separate Topics)
    /// </summary>
    public static class Auth
    {
        public const string AccountCreated = "emis.auth.account.created";
        public const string LoginFailed = "emis.auth.login.failed";
    }
}

#endregion

#region Publisher Example

/// <summary>
/// Publisher publishes to aggregate topic with partition key
/// </summary>
public class StudentCommandHandler
{
    private readonly IEventBus _eventBus;

    public async Task CreateStudentAsync(CreateStudentCommand command)
    {
        // 1. Save student to database
        var student = new Student(command.FullName, command.Email);
        await _repository.AddAsync(student);
        await _unitOfWork.SaveChangesAsync();

        // 2. Publish event to aggregate topic
        var @event = new StudentCreatedEvent
        {
            StudentId = student.Id,
            FullName = student.FullName,
            Email = student.Email,
            // EventType = "student.created" automatically set
        };

        // Publish to "emis.student" topic
        // Partition key = StudentId (ensures ordering per student)
        await _eventBus.PublishAsync(@event, partitionKey: @event.StudentId.ToString());
    }
}

#endregion

#region Consumer Example - Pattern Matching

/// <summary>
/// Consumer subscribes to aggregate topic and filters by EventType
/// </summary>
public class StudentEventConsumer : IEventHandler<AggregateEvent>
{
    public async Task HandleAsync(AggregateEvent @event, CancellationToken cancellationToken)
    {
        // Pattern matching based on EventType
        switch (@event.EventType)
        {
            case "student.created":
                await HandleStudentCreatedAsync((StudentCreatedEvent)@event, cancellationToken);
                break;

            case "student.updated":
                await HandleStudentUpdatedAsync((StudentUpdatedEvent)@event, cancellationToken);
                break;

            case "student.enrolled":
                await HandleStudentEnrolledAsync((StudentEnrolledEvent)@event, cancellationToken);
                break;

            default:
                // Ignore unknown event types (forward compatibility)
                _logger.LogWarning("Unknown event type: {EventType}", @event.EventType);
                break;
        }
    }

    private async Task HandleStudentCreatedAsync(StudentCreatedEvent @event, CancellationToken ct)
    {
        // Create authentication account
        // Send welcome notification
        // etc.
    }

    private async Task HandleStudentUpdatedAsync(StudentUpdatedEvent @event, CancellationToken ct)
    {
        // Update local replica
        // etc.
    }

    private async Task HandleStudentEnrolledAsync(StudentEnrolledEvent @event, CancellationToken ct)
    {
        // Update class roster
        // etc.
    }
}

#endregion

#region Consumer Example - Specific Event Filter

/// <summary>
/// Consumer that only cares about specific events
/// Filters others out immediately
/// </summary>
public class AuthServiceStudentConsumer : IEventHandler<AggregateEvent>
{
    // AuthService only cares about student.created
    private static readonly HashSet<string> InterestedEventTypes = new()
    {
        "student.created"
    };

    public async Task HandleAsync(AggregateEvent @event, CancellationToken cancellationToken)
    {
        // Quick filter - ignore uninteresting events
        if (!InterestedEventTypes.Contains(@event.EventType))
        {
            return; // Skip processing
        }

        // Process only student.created
        if (@event is StudentCreatedEvent created)
        {
            await CreateAuthAccountAsync(created, cancellationToken);
        }
    }

    private async Task CreateAuthAccountAsync(StudentCreatedEvent @event, CancellationToken ct)
    {
        // Create account logic
    }
}

#endregion

#region KafkaEventBus Enhancement - Partition Key Support

/// <summary>
/// Enhanced EventBus interface with partition key support
/// </summary>
public interface IEventBusEnhanced
{
    /// <summary>
    /// Publish event to topic with partition key
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, string? partitionKey = null, CancellationToken ct = default)
        where TEvent : IEvent;
}

/// <summary>
/// Kafka implementation with partition key
/// </summary>
public class KafkaEventBusEnhanced : IEventBusEnhanced
{
    public async Task PublishAsync<TEvent>(TEvent @event, string? partitionKey = null, CancellationToken ct = default)
        where TEvent : IEvent
    {
        var topic = GetTopicName(@event);

        var message = new Message<string, string>
        {
            Key = partitionKey ?? Guid.NewGuid().ToString(), // Use partition key if provided
            Value = JsonSerializer.Serialize(@event)
        };

        await _producer.ProduceAsync(topic, message, ct);
    }

    private string GetTopicName<TEvent>(TEvent @event) where TEvent : IEvent
    {
        // For aggregate events, return aggregate topic
        if (@event is StudentCreatedEvent || @event is StudentUpdatedEvent || @event is StudentEnrolledEvent)
            return TopicNamesHybrid.Student.Events;

        // For critical student events, return separate topic
        if (@event is StudentGraduatedEvent)
            return TopicNamesHybrid.Student.Graduated;

        // Default: use EventType as topic name
        return @event.EventType;
    }
}

#endregion

#region Configuration - Kafka Consumer for Aggregate Topics

/// <summary>
/// Consumer configuration for aggregate topics
/// </summary>
public static class KafkaConsumerConfiguration
{
    public static void ConfigureAggregateConsumers(this IServiceCollection services)
    {
        services.AddKafkaConsumer(consumer =>
        {
            // Subscribe to Student aggregate topic
            // This topic contains: student.created, student.updated, student.enrolled, etc.
            consumer.Subscribe(TopicNamesHybrid.Student.Events);

            // Subscribe to critical Student event topic
            consumer.Subscribe(TopicNamesHybrid.Student.Graduated);

            // Subscribe to Teacher aggregate topic
            consumer.Subscribe(TopicNamesHybrid.Teacher.Events);

            // Subscribe to high-volume separate topics
            consumer.Subscribe(TopicNamesHybrid.Attendance.Marked);
        });

        // Register event handlers
        services.AddScoped<IEventHandler<AggregateEvent>, StudentEventConsumer>();
        services.AddScoped<IEventHandler<StudentGraduatedEvent>, StudentGraduationHandler>();
    }
}

#endregion

#region Comparison: Current vs Hybrid

/// <summary>
/// COMPARISON
/// 
/// Current Strategy (Topic per Event Type):
/// ==========================================
/// Topics:
///   - emis.student.created
///   - emis.student.updated
///   - emis.student.enrolled
///   - emis.student.graduated
///   Total: 4 topics × 10 partitions = 40 partitions
/// 
/// Publisher:
///   await _eventBus.PublishAsync(new StudentCreatedEvent {...});
///   // Automatically routes to "emis.student.created" topic
/// 
/// Consumer:
///   services.AddEventHandler<StudentCreatedEvent, StudentCreatedHandler>();
///   // Simple, type-safe, but many handlers
/// 
/// Pros:
///   ✅ Simple, clear semantics
///   ✅ Type-safe consumers
///   ✅ Easy to understand
/// Cons:
///   ❌ More topics and partitions
///   ❌ No ordering guarantee across event types
/// 
/// 
/// Hybrid Strategy (Topic per Aggregate):
/// ========================================
/// Topics:
///   - emis.student (contains: created, updated, enrolled)
///   - emis.student.graduated (critical, separate)
///   Total: 2 topics × 10 partitions = 20 partitions
/// 
/// Publisher:
///   await _eventBus.PublishAsync(new StudentCreatedEvent {...}, 
///                                partitionKey: student.Id);
///   // Routes to "emis.student" topic, partition by StudentId
/// 
/// Consumer:
///   services.AddEventHandler<AggregateEvent, StudentEventConsumer>();
///   // Single handler, pattern matching inside
/// 
/// Pros:
///   ✅ Fewer topics and partitions (50% reduction)
///   ✅ Ordering guarantee per student
///   ✅ Can evolve schema more easily
/// Cons:
///   ❌ Consumer must filter events
///   ❌ More complex consumer logic
///   ❌ Less type-safe
/// 
/// 
/// Recommendation for EMIS:
/// ========================
/// - Start with Current Strategy (Topic per Event Type)
/// - Monitor for 3-6 months
/// - If you hit 100+ topics or partition limits, migrate to Hybrid
/// - Migrate incrementally: low-volume aggregates first
/// </summary>

#endregion
