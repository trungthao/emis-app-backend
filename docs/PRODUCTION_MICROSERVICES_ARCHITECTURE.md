# Production-Ready Microservices Architecture

## Overview
Đây là tài liệu về kiến trúc microservices production-ready cho hệ thống EMIS, phục vụ **25,000+ trường học** và **25,000,000+ người dùng** trên toàn quốc Việt Nam.

## Problem Statement

### ❌ Anti-Patterns (Không chấp nhận được trong Production)

1. **Mock Data**
   - Không có data thực → Không test được
   - Không phản ánh production → Bug ẩn
   - Không scalable → Dead on arrival

2. **Synchronous HTTP Calls Between Services**
   ```
   TeacherService → HTTP → ClassService (for each class)
   ```
   - **N+1 Query Problem**: Teacher có 10 classes = 10 HTTP calls
   - **High Latency**: Mỗi call ~100ms → 1 second total
   - **Cascading Failures**: ClassService down → TeacherService down
   - **No Caching**: Database overload
   - **Network Overhead**: Bandwidth waste

3. **Tight Coupling**
   - Services depend on each other synchronously
   - Cannot deploy independently
   - Cannot scale independently

## ✅ Production Solution: Event-Driven + Local Replica Pattern

### Architecture Diagram

```
┌─────────────────────┐         ┌─────────────────────┐
│   ClassService      │         │   TeacherService    │
│  (Source of Truth)  │         │  (Consumer)         │
└─────────────────────┘         └─────────────────────┘
          │                                │
          │ 1. Create/Update Class         │
          ├────────────────────────────────┤
          │                                │
          │ 2. Publish Event               │
          ▼                                │
┌─────────────────────────────────────────┐│
│           Kafka Cluster                 ││
│  Topic: emis.class.created/updated      ││
│  Partitions: 10, RF=3                   ││
└─────────────────────────────────────────┘│
                    │                       │
                    │ 3. Consume Event      │
                    └───────────────────────▼
                        ┌─────────────────────────────┐
                        │ ClassCreatedEventHandler    │
                        │ ClassUpdatedEventHandler    │
                        └─────────────────────────────┘
                                    │
                                    │ 4. Upsert to Local DB
                                    ▼
                        ┌─────────────────────────────┐
                        │  TeacherService Database    │
                        │  ┌─────────────────────┐    │
                        │  │ ClassInfos Table    │    │
                        │  │ (Local Replica)     │    │
                        │  └─────────────────────┘    │
                        └─────────────────────────────┘
                                    │
                                    │ 5. Query (No Network Call!)
                                    ▼
                        ┌─────────────────────────────┐
                        │ GetTeacherDetailQuery       │
                        │ - 1 DB query (batch load)   │
                        │ - No HTTP calls             │
                        │ - Sub-millisecond latency   │
                        └─────────────────────────────┘
```

### Key Components

#### 1. **ClassInfo Entity** (Local Replica)

```csharp
public class ClassInfo : BaseEntity
{
    public Guid ClassId { get; private set; }      // External ID
    public string ClassName { get; private set; }
    public string? Grade { get; private set; }
    public string? AcademicYear { get; private set; }
    public int? TotalStudents { get; private set; }
    public DateTime LastSyncedAt { get; private set; }  // For monitoring
    public string? SyncSource { get; private set; }     // For debugging
}
```

**Purpose**: Store local copy of class information from ClassService

**Benefits**:
- ✅ No network latency (local database query)
- ✅ No single point of failure (TeacherService independent)
- ✅ Batch queries (1 query for N classes)
- ✅ Always available (eventual consistency)

#### 2. **Event Handlers** (Data Synchronization)

**ClassCreatedEventHandler**:
```csharp
public class ClassCreatedEventHandler : IEventHandler<ClassCreatedEvent>
{
    public async Task HandleAsync(ClassCreatedEvent @event, ...)
    {
        // Create local replica
        var classInfo = new ClassInfo(...);
        await _repository.UpsertClassInfoAsync(classInfo);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

**ClassUpdatedEventHandler**:
```csharp
public class ClassUpdatedEventHandler : IEventHandler<ClassUpdatedEvent>
{
    public async Task HandleAsync(ClassUpdatedEvent @event, ...)
    {
        // Update local replica
        var classInfo = new ClassInfo(...);
        await _repository.UpsertClassInfoAsync(classInfo);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

**Purpose**: Keep local replica synchronized with source of truth

**Flow**:
1. ClassService creates/updates class
2. Publishes event to Kafka
3. TeacherService consumes event
4. Updates local ClassInfo table
5. Data now available for queries

#### 3. **Query Handler** (Optimized Read)

**Before (Mock Data)**:
```csharp
// ❌ Mock data - không production
private async Task<ClassInfoDto> GetClassInfoByIdAsync(Guid classId)
{
    return mockData; // FAKE!
}
```

**After (Local Replica)**:
```csharp
// ✅ Production-ready
public async Task<TeacherDetailDto> Handle(GetTeacherDetailQuery request, ...)
{
    // Get teacher
    var teacher = await _repository.GetByIdAsync(request.TeacherId);
    
    // Get assignments
    var assignments = await _repository.GetAssignmentsByTeacherIdAsync(request.TeacherId);
    
    // Batch load class info (1 query for ALL classes!)
    var classIds = assignments.Select(a => a.ClassId).Distinct().ToList();
    var classInfos = await _repository.GetClassInfosByIdsAsync(classIds); // ← MAGIC!
    var classInfoDict = classInfos.ToDictionary(c => c.ClassId);
    
    // Map data
    foreach (var assignment in assignments)
    {
        var classInfo = classInfoDict.GetValueOrDefault(assignment.ClassId);
        // Use classInfo (no HTTP call, no latency!)
    }
}
```

**Performance**:
- **Queries**: 3 total (teacher, assignments, class infos)
- **Network calls**: 0
- **Latency**: <10ms (vs 1000ms with HTTP calls)
- **Scalability**: Infinite (database can handle it)

## Benefits

### 1. **Performance** 🚀

| Metric | Mock/HTTP Approach | Event-Driven Approach |
|--------|-------------------|----------------------|
| Network calls | 1 + N classes | 0 |
| Latency | ~100ms per class | <1ms |
| Total time (10 classes) | ~1000ms | ~10ms |
| Database load | N+1 queries | 1 batch query |
| Scalability | Limited | Unlimited |

### 2. **Reliability** 💪

**Problem**: ClassService is down
- ❌ **HTTP Approach**: TeacherService also down (cascading failure)
- ✅ **Event-Driven**: TeacherService still works (eventual consistency)

**Problem**: Network latency spike
- ❌ **HTTP Approach**: All requests slow
- ✅ **Event-Driven**: No impact (local DB)

### 3. **Scalability** 📈

**Load**: 10,000 concurrent requests to GetTeacherDetail
- ❌ **HTTP Approach**: 10,000 × N HTTP calls to ClassService → Overload
- ✅ **Event-Driven**: 10,000 local DB queries → No problem

**Data**: 1,000,000 classes nationwide
- ❌ **HTTP Approach**: ClassService becomes bottleneck
- ✅ **Event-Driven**: Each TeacherService has only its relevant classes (~100-1000)

### 4. **Cost** 💰

**Network bandwidth** (assuming 1KB per HTTP response):
- ❌ **HTTP Approach**: 1 million requests/day × 10 classes × 1KB = 10GB/day
- ✅ **Event-Driven**: Only sync when class changes (~1MB/day)

**Compute cost**:
- ❌ **HTTP Approach**: More instances needed to handle HTTP traffic
- ✅ **Event-Driven**: Minimal compute (event processing is async)

## Implementation Details

### Database Schema

```sql
CREATE TABLE ClassInfos (
    Id CHAR(36) PRIMARY KEY,
    ClassId CHAR(36) NOT NULL UNIQUE,  -- External ID from ClassService
    ClassName VARCHAR(200) NOT NULL,
    Grade VARCHAR(50),
    AcademicYear VARCHAR(20),
    TotalStudents INT,
    SchoolId CHAR(36),
    LastSyncedAt DATETIME NOT NULL,
    SyncSource VARCHAR(100),
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    INDEX idx_classid (ClassId)
);
```

### Kafka Topics

```
Topic: emis.class.created
- Partitions: 10
- Replication Factor: 3
- Min ISR: 2
- Retention: 7 days
- Consumer Group: emis-teacher-service

Topic: emis.class.updated
- Partitions: 10
- Replication Factor: 3
- Min ISR: 2
- Retention: 7 days
- Consumer Group: emis-teacher-service
```

### Event Schema

```json
{
  "eventId": "uuid",
  "eventType": "emis.class.created",
  "timestamp": "2025-10-26T10:00:00Z",
  "classId": "uuid",
  "className": "Lớp 10A1",
  "grade": "Khối 10",
  "academicYear": "2024-2025",
  "totalStudents": 45,
  "schoolId": "uuid",
  "homeroomTeacherId": "uuid"
}
```

## Consistency Model

### Eventual Consistency

**Scenario**: ClassService creates new class

```
T0: ClassService creates class "10A1" in database
T1: ClassService publishes ClassCreatedEvent to Kafka
T2: Event persisted in Kafka (durability guaranteed)
T3: TeacherService consumes event (within seconds)
T4: TeacherService updates local ClassInfo table
T5: Data now available for queries

Total time: ~1-2 seconds (acceptable for this use case)
```

**Guarantees**:
- ✅ **At-least-once delivery**: Event will be processed (Kafka durability + manual offset commit)
- ✅ **Idempotency**: Upsert operation (safe to replay)
- ✅ **Ordering**: Per-partition ordering (same ClassId goes to same partition via key)

### Handling Inconsistencies

**Scenario 1**: Event lost (extremely rare with Kafka RF=3)
- **Detection**: Monitoring shows ClassInfo.LastSyncedAt too old
- **Resolution**: Periodic batch sync job (nightly)

**Scenario 2**: TeacherService was down during event
- **Behavior**: Kafka retains event (7-day retention)
- **Resolution**: Consume from earliest offset when service restarts

**Scenario 3**: Class deleted but TeacherService still has replica
- **Solution**: Implement ClassDeletedEvent
- **Handler**: Soft delete or hard delete local replica

## Monitoring & Operations

### Metrics to Monitor

1. **Consumer Lag**
   ```bash
   docker exec emis-kafka-1 kafka-consumer-groups \
     --bootstrap-server kafka-1:29092 \
     --describe \
     --group emis-teacher-service
   ```
   - **Alert if**: Lag > 1000 messages
   - **Action**: Scale consumers or investigate slow processing

2. **Sync Latency**
   - Measure time from event timestamp to local update
   - **Alert if**: Average > 5 seconds
   - **Action**: Check Kafka cluster health

3. **Data Freshness**
   ```sql
   SELECT ClassId, LastSyncedAt, 
          TIMESTAMPDIFF(MINUTE, LastSyncedAt, NOW()) as MinutesSinceSync
   FROM ClassInfos
   WHERE LastSyncedAt < DATE_SUB(NOW(), INTERVAL 1 HOUR);
   ```
   - **Alert if**: Any class not synced in 1 hour
   - **Action**: Check event publishing or consumer health

### Operational Runbook

**Problem**: Consumer lag increasing

1. Check Kafka cluster health
2. Check consumer processing time (logs)
3. Scale consumer instances (horizontal)
4. Check database connection pool
5. Investigate slow queries

**Problem**: ClassInfo data seems outdated

1. Check ClassService is publishing events
2. Check Kafka topic has messages
3. Check consumer is running
4. Check consumer logs for errors
5. Manual sync if needed

## Migration Strategy

### Phase 1: Parallel Run (Week 1)
- Deploy new code with local replica
- Keep old code running
- Compare results: mock vs real data
- Monitor performance

### Phase 2: Shadow Mode (Week 2)
- New code primary
- Log differences if any
- Build confidence

### Phase 3: Full Cutover (Week 3)
- Remove mock data code
- 100% on local replica
- Monitor closely

### Phase 4: Optimization (Week 4)
- Add caching layer (Redis) if needed
- Fine-tune consumer settings
- Optimize database queries

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task ClassCreatedEventHandler_Should_CreateLocalReplica()
{
    // Arrange
    var @event = new ClassCreatedEvent { ClassId = Guid.NewGuid(), ... };
    var handler = new ClassCreatedEventHandler(...);
    
    // Act
    await handler.HandleAsync(@event);
    
    // Assert
    var classInfo = await _repository.GetClassInfoByIdAsync(@event.ClassId);
    Assert.NotNull(classInfo);
    Assert.Equal(@event.ClassName, classInfo.ClassName);
}
```

### Integration Tests
```csharp
[Fact]
public async Task GetTeacherDetail_Should_LoadClassInfoFromLocalReplica()
{
    // Arrange: Seed ClassInfo
    await SeedClassInfoAsync();
    
    // Act
    var result = await _mediator.Send(new GetTeacherDetailQuery { TeacherId = ... });
    
    // Assert
    Assert.NotEmpty(result.ClassAssignments);
    Assert.Equal("Lớp 10A1", result.ClassAssignments[0].ClassName);
}
```

### Load Tests
```bash
# 10,000 concurrent requests
ab -n 10000 -c 100 http://localhost:5000/api/teachers/{id}

# Expected results:
# - Average latency: <50ms
# - 99th percentile: <100ms
# - 0% errors
```

## Comparison with Alternatives

### Alternative 1: Synchronous HTTP Calls
❌ High latency, tight coupling, cascading failures

### Alternative 2: GraphQL Federation
❌ Complex infrastructure, still has latency, requires GraphQL gateway

### Alternative 3: Shared Database
❌ Violates microservices principles, tight coupling at data level

### Alternative 4: Event-Driven + Local Replica ✅
- Low latency (local queries)
- Loose coupling (services independent)
- High availability (no cascading failures)
- Scalable (horizontal scaling)
- Cost-effective (less network traffic)

## Conclusion

For a **production system** serving **25 million users** across **25,000 schools**, the Event-Driven + Local Replica pattern is the ONLY viable solution.

**Key Principles**:
1. ✅ **No mock data** - Always use real data
2. ✅ **No synchronous inter-service calls for reads** - Use local replicas
3. ✅ **Embrace eventual consistency** - Acceptable for most business cases
4. ✅ **Design for failure** - Services must be independent
5. ✅ **Optimize for scalability** - Think 100x current load

This is **production-ready architecture** used by companies like Netflix, Uber, LinkedIn for systems serving hundreds of millions of users.
