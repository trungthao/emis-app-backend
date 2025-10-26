# Kafka Topic Strategy - EMIS System

## 📋 Tổng quan

Document này phân tích và đề xuất chiến lược tổ chức Kafka topics cho hệ thống EMIS phục vụ 25 triệu người dùng, 25,000 trường học trên toàn quốc Việt Nam.

## 🎯 Hai chiến lược chính

### Chiến lược 1: Topic per Event Type (hiện tại)

```
emis.student.created
emis.student.updated
emis.student.enrolled
emis.student.graduated
emis.student.transferred
emis.student.deleted
...
→ Mỗi event type = 1 topic riêng
→ Tổng: ~100-300 topics
```

**Ưu điểm:**
- ✅ Consumer subscribe chính xác event cần thiết
- ✅ Schema đơn giản, mỗi topic có 1 schema cố định
- ✅ Easy monitoring per event type
- ✅ Clear separation of concerns

**Nhược điểm:**
- ❌ Nhiều topics (100-300+)
- ❌ Nhiều partitions (1000-3000+)
- ❌ Không đảm bảo event ordering giữa các event types
- ❌ Topic sprawl - khó quản lý khi scale

### Chiến lược 2: Topic per Aggregate/Domain

```
emis.student (1 topic)
  → student.created (event type field)
  → student.updated
  → student.enrolled
  → student.graduated
  ...

→ Mỗi aggregate/domain = 1 topic
→ Tổng: ~15-30 topics
```

**Ưu điểm:**
- ✅ Ít topics hơn nhiều (15-30 vs 100-300)
- ✅ Ít partitions (150-300 vs 1000-3000)
- ✅ Đảm bảo event ordering trong cùng aggregate
- ✅ Dễ quản lý, monitoring

**Nhược điểm:**
- ❌ Consumer phải filter events không cần thiết
- ❌ Schema phức tạp hơn (union types)
- ❌ Một consumer chậm ảnh hưởng toàn bộ topic
- ❌ Cần partition key strategy cẩn thận

## 🌍 Best Practices từ các công ty lớn

### Netflix
```yaml
Strategy: Topic per Aggregate
Topics: ~200 topics
Example:
  - studio-events (all studio domain events)
  - user-events (all user domain events)
  - viewing-events (all viewing domain events)
  
Approach:
  - Use event.type field to distinguish
  - Partition by entity ID (userId, studioId)
  - Consumer filters by event.type
```

### Uber
```yaml
Strategy: Hybrid
Topics: ~500 topics
Example:
  - uber.trip (aggregate topic)
  - uber.driver (aggregate topic)
  - uber.location.updated (separate - high volume)
  - uber.payment.completed (separate - critical)

Approach:
  - Core aggregates: 1 topic per aggregate
  - High-volume: separate topics
  - Critical events: separate topics
```

### LinkedIn
```yaml
Strategy: Topic per Aggregate
Topics: ~200 topics
Example:
  - MemberProfile (all member events)
  - JobPosting (all job events)
  - Messaging (all message events)

Approach:
  - ~200 topics for hundreds of services
  - Strict governance and naming convention
```

### Shopify
```yaml
Strategy: Topic per Event Type
Topics: ~1000+ topics
Example:
  - orders.created
  - orders.updated
  - orders.cancelled
  - products.created

Approach:
  - More topics but very clear semantics
  - Heavy investment in tooling and governance
```

## 🎯 Khuyến nghị cho EMIS: HYBRID Strategy

### Phân loại Events

```
┌─────────────────────────────────────────────────────────────┐
│  EVENT CLASSIFICATION                                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. HIGH-VOLUME, LOW-CRITICALITY                           │
│     → Topic per Aggregate                                   │
│     Examples:                                               │
│     - emis.attendance (millions/day)                       │
│     - emis.notification (millions/day)                     │
│                                                             │
│  2. LOW-VOLUME, HIGH-CRITICALITY                           │
│     → Topic per Event Type                                  │
│     Examples:                                               │
│     - emis.student.graduated                               │
│     - emis.auth.account.created                            │
│                                                             │
│  3. MODERATE-VOLUME, MODERATE-CRITICALITY                  │
│     → Topic per Aggregate (with sub-topics for critical)   │
│     Examples:                                               │
│     - emis.student (aggregate)                             │
│       + emis.student.graduated (critical sub-topic)        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Đề xuất cụ thể

```csharp
public static class TopicNames
{
    /// <summary>
    /// TIER 1: Core Entity Events (Topic per Aggregate)
    /// Low-medium volume, need ordering guarantee
    /// </summary>
    public static class Student
    {
        // Main aggregate topic
        public const string Events = "emis.student";
        
        // Critical events get separate topics
        public const string Graduated = "emis.student.graduated";
    }
    
    public static class Teacher
    {
        public const string Events = "emis.teacher";
        // No separate topics needed - moderate criticality
    }
    
    public static class Class
    {
        public const string Events = "emis.class";
    }
    
    /// <summary>
    /// TIER 2: High-Volume Events (Separate Topics)
    /// Millions of events per day
    /// </summary>
    public static class Attendance
    {
        // Separate topics due to high volume
        public const string Marked = "emis.attendance.marked";
        public const string AbsenceReported = "emis.attendance.absence.reported";
        public const string LateArrival = "emis.attendance.late.arrival";
    }
    
    /// <summary>
    /// TIER 3: Critical Business Events (Separate Topics)
    /// Need special handling, monitoring, retry logic
    /// </summary>
    public static class Auth
    {
        // Separate topics - critical for security
        public const string AccountCreated = "emis.auth.account.created";
        public const string LoginFailed = "emis.auth.login.failed";
        public const string PasswordChanged = "emis.auth.password.changed";
    }
    
    public static class Grade
    {
        // Separate topics - critical for academic records
        public const string Published = "emis.grade.published";
        public const string Finalized = "emis.grade.finalized";
    }
    
    /// <summary>
    /// TIER 4: Infrastructure Events (Topic per Aggregate)
    /// Internal events, moderate volume
    /// </summary>
    public static class Notification
    {
        public const string Events = "emis.notification";
    }
}
```

### Event Structure trong Aggregate Topic

```csharp
// Base event cho aggregate topic
public abstract class BaseEvent : IEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public abstract string EventType { get; }  // "student.created", "student.updated"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;
}

// Specific events
public class StudentCreatedEvent : BaseEvent
{
    public override string EventType => "student.created";
    public Guid StudentId { get; set; }
    // ... other fields
}

public class StudentUpdatedEvent : BaseEvent
{
    public override string EventType => "student.updated";
    public Guid StudentId { get; set; }
    // ... other fields
}

// Consumer filtering
public class StudentEventHandler : IEventHandler<BaseEvent>
{
    public async Task HandleAsync(BaseEvent @event, CancellationToken ct)
    {
        switch (@event)
        {
            case StudentCreatedEvent created:
                await HandleCreatedAsync(created, ct);
                break;
            case StudentUpdatedEvent updated:
                await HandleUpdatedAsync(updated, ct);
                break;
            // ... other event types
        }
    }
}
```

## 📊 Ước tính số lượng Topics

### Current Strategy (Topic per Event Type)

```
Domain              Event Types    Topics    Partitions (×10)
─────────────────────────────────────────────────────────────
Student             8              8         80
Teacher             6              6         60
Class               3              3         30
Parent              5              5         50
Attendance          3              3         30
Grade               4              4         40
Auth                6              6         60
Notification        5              5         50
Assignment          4              4         40
Schedule            3              3         30
Report              4              4         40
Audit               3              3         30
─────────────────────────────────────────────────────────────
TOTAL                              54        540 partitions
```

### Recommended Strategy (Hybrid)

```
Domain              Topics         Partitions (×10)
────────────────────────────────────────────────────
TIER 1: Aggregates
Student             1              10
Teacher             1              10
Class               1              10
Parent              1              10
Assignment          1              10

TIER 2: High-Volume
Attendance          3              30
Notification        1              20 (×20 partitions)

TIER 3: Critical
Student.Graduated   1              5 (×5 partitions)
Grade.Published     1              10
Grade.Finalized     1              10
Auth.AccountCreated 1              10
Auth.LoginFailed    1              10
Auth.PasswordChanged 1             10

TIER 4: Infrastructure
Report              1              10
Audit               1              10
Schedule            1              10
────────────────────────────────────────────────────
TOTAL               17             185 partitions
```

**Reduction: 54 → 17 topics (68% fewer)**
**Reduction: 540 → 185 partitions (66% fewer)**

## ⚡ Performance Implications

### Kafka Cluster Resources

```
Current Strategy:
- 54 topics × 10 partitions = 540 partitions
- Memory: 540 partitions × 1 MB = 540 MB
- ZooKeeper metadata: ~50 MB
- Controller election time: ~10-15s
→ Acceptable but not optimal

Hybrid Strategy:
- 17 topics × ~11 partitions avg = 185 partitions
- Memory: 185 partitions × 1 MB = 185 MB
- ZooKeeper metadata: ~20 MB
- Controller election time: ~3-5s
→ Much better resource utilization
```

### Throughput Analysis

```
Scenario: 25M users, 25K schools

Daily Events Estimate:
─────────────────────────────────────────────────────
Attendance.Marked:        5M events/day (peak: 50K/s during school hours)
Notification.Events:      10M events/day (100/s average)
Student.Events:          100K events/day (1/s average)
Teacher.Events:          10K events/day (0.1/s average)
Grade.Published:         500K events/day (5/s average)
Auth.Login:              2M events/day (20/s average)

With Hybrid Strategy:
✅ High-volume events (Attendance) have separate topics → no bottleneck
✅ Low-volume aggregates share topics → efficient resource usage
✅ Critical events separated → special handling possible
```

## 🔧 Implementation Strategy

### Phase 1: Current State (Keep as-is for now)
```
✅ Continue with Topic per Event Type
✅ Monitor Kafka metrics (partition count, throughput)
✅ Set up governance and naming conventions
✅ Build tooling for topic management
```

### Phase 2: Identify High-Volume Events (Month 2-3)
```
- Collect metrics on event volumes
- Identify topics with >1000 events/second
- These remain separate topics
```

### Phase 3: Consolidate Low-Volume Events (Month 3-6)
```
- Merge low-volume events (<10 events/second) into aggregate topics
- Example: Student events → emis.student aggregate
- Maintain backward compatibility with aliases
```

### Phase 4: Optimize Critical Path (Month 6-12)
```
- Separate critical events (graduation, grade finalization)
- Implement enhanced monitoring and alerting
- Add retry/DLQ strategies per criticality tier
```

## 🎯 Decision Matrix

**Use Topic per Event Type when:**
- ✅ Event volume > 1000/second
- ✅ Event is business-critical (graduation, payment)
- ✅ Event needs special monitoring/alerting
- ✅ Event consumed by many different services with different SLAs

**Use Topic per Aggregate when:**
- ✅ Event volume < 100/second
- ✅ Events need ordering guarantee (student lifecycle)
- ✅ Events closely related (CRUD operations)
- ✅ Small number of consumers

## 📝 Recommended Action for EMIS

### Short-term (Next 3 months)
```
✅ KEEP current "Topic per Event Type" strategy
✅ Reason: Simpler to implement and understand
✅ 54 topics, 540 partitions is totally manageable
✅ Focus on building features, not premature optimization
```

### Medium-term (3-12 months)
```
⚡ Monitor and collect metrics
⚡ Identify high-volume topics (attendance, notifications)
⚡ Identify low-volume topics (parent, class)
⚡ Plan migration to hybrid strategy
```

### Long-term (12+ months)
```
🎯 Migrate to Hybrid Strategy:
   - 5-10 aggregate topics (low-volume domains)
   - 5-10 separate topics (high-volume or critical)
   - Total: 15-20 topics, 150-250 partitions
```

## 🔍 Monitoring & Governance

```yaml
Topic Governance Rules:
  - Naming: emis.<domain>.<event> or emis.<domain>
  - Partitions: Default 10, high-volume 20-50
  - Replication Factor: 3 (production)
  - min.insync.replicas: 2
  - Retention: 7 days (default), 30 days (audit)
  
Metrics to Monitor:
  - Topic count (alert if > 100)
  - Partition count (alert if > 1000)
  - Consumer lag per topic
  - Events/second per topic
  - Disk usage per topic
  
Review Cycle:
  - Quarterly review of topic strategy
  - Identify candidates for consolidation
  - Identify candidates for separation
```

## 📚 References

- [Confluent: How Many Topics](https://www.confluent.io/blog/how-choose-number-topics-partitions-kafka-cluster/)
- [Netflix: Event Data Pipeline](https://netflixtechblog.com/evolution-of-the-netflix-data-pipeline-da246ca36905)
- [Uber: Reliable Reprocessing](https://eng.uber.com/reliable-reprocessing/)
- [LinkedIn: Kafka at Scale](https://engineering.linkedin.com/kafka/running-kafka-scale)

---

**Kết luận:**
- ✅ Current strategy (54 topics) is FINE for now
- ⚡ Consider Hybrid strategy when you have real metrics
- 🎯 Don't optimize prematurely - focus on features first
- 📊 Set up monitoring and review quarterly
