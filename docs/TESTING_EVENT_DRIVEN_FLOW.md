# Testing Event-Driven Flow Guide

## Overview
Hướng dẫn test end-to-end flow của Event-Driven Architecture giữa TeacherService (Publisher) và AuthService (Consumer).

## Architecture Flow
```
1. Admin creates teacher via POST /api/teachers
   ↓
2. TeacherService saves teacher to database
   ↓
3. CreateTeacherCommandHandler publishes TeacherCreatedEvent to Kafka
   ↓
4. Kafka topic: emis.auth.teacher.created (10 partitions, RF=3)
   ↓
5. AuthService consumes event from Kafka
   ↓
6. TeacherCreatedEventHandler creates user account
   ↓
7. Teacher can login with generated credentials
```

## Prerequisites

### 1. Kafka Cluster Running
```bash
# Check all Kafka brokers are healthy
docker ps --filter "name=emis-kafka" --format "table {{.Names}}\t{{.Status}}"

# Should show:
# emis-kafka-1    Up X minutes (healthy)
# emis-kafka-2    Up X minutes (healthy)
# emis-kafka-3    Up X minutes (healthy)
# emis-kafka-ui   Up X hours (healthy)
```

### 2. Kafka Topic Created
```bash
# Verify topic exists
docker exec emis-kafka-1 kafka-topics --list --bootstrap-server kafka-1:29092

# Check topic details
docker exec emis-kafka-1 kafka-topics --describe --bootstrap-server kafka-1:29092 --topic emis.auth.teacher.created

# Expected output:
# Topic: emis.auth.teacher.created
# PartitionCount: 10
# ReplicationFactor: 3
# min.insync.replicas=2
# retention.ms=604800000 (7 days)
```

### 3. Databases Running
```bash
# Check MySQL containers
docker ps --filter "name=emis-teacher-db" --format "table {{.Names}}\t{{.Status}}"
docker ps --filter "name=emis-auth-db" --format "table {{.Names}}\t{{.Status}}"
```

## Test Steps

### Step 1: Start TeacherService (Publisher)

```bash
cd /Users/trungthao/Projects/emis-app-backend
dotnet run --project src/Services/TeacherService/Teacher.API/Teacher.API.csproj
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Verify EventBus registration:**
- Log should show: "Kafka EventBus registered with bootstrap servers: localhost:9092,localhost:9093,localhost:9094"

### Step 2: Start AuthService (Consumer)

Open a **new terminal**:

```bash
cd /Users/trungthao/Projects/emis-app-backend
dotnet run --project src/Services/AuthService/Auth.API/Auth.API.csproj
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5002
info: EMIS.EventBus.Kafka.Kafka.KafkaConsumerService[0]
      Starting Kafka Consumer Service
info: EMIS.EventBus.Kafka.Kafka.KafkaConsumerService[0]
      Subscribed to topics: emis.auth.teacher.created
```

**Verify Consumer subscription:**
- Log should show: "Subscribed to topics: emis.auth.teacher.created"
- Log should show: "Kafka Consumer Service started"

### Step 3: Create Teacher via API

```bash
# Using curl
curl -X POST http://localhost:5000/api/teachers \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Nguyen Van",
    "lastName": "A",
    "email": "nguyenvana@school.edu.vn",
    "phone": "0901234567",
    "dateOfBirth": "1985-05-15",
    "address": "123 Nguyen Hue, District 1, Ho Chi Minh City"
  }'
```

**Or using REST Client** (create file `Teacher-EventBus-Test.http`):

```http
### Create Teacher (Test Event Publishing)
POST http://localhost:5000/api/teachers
Content-Type: application/json

{
  "firstName": "Nguyen Van",
  "lastName": "B",
  "email": "nguyenvanb@school.edu.vn",
  "phone": "0907654321",
  "dateOfBirth": "1988-08-20",
  "address": "456 Le Loi, District 3, Ho Chi Minh City"
}
```

**Expected HTTP Response: 200 OK**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Nguyen Van",
  "lastName": "B",
  "fullName": "Nguyen Van B",
  "email": "nguyenvanb@school.edu.vn",
  "phone": "0907654321",
  "dateOfBirth": "1988-08-20T00:00:00",
  "createdAt": "2025-10-26T01:30:00",
  "status": "Active"
}
```

### Step 4: Verify Event Published (TeacherService Logs)

**TeacherService console should show:**

```
info: Teacher.Application.UseCases.Teachers.Commands.CreateTeacher.CreateTeacherCommandHandler[0]
      Published TeacherCreatedEvent for Teacher 3fa85f64-5717-4562-b3fc-2c963f66afa6 with email nguyenvanb@school.edu.vn

info: EMIS.EventBus.Kafka.Kafka.KafkaEventBus[0]
      Publishing event TeacherCreatedEvent (ID: 7b2c1234-...) to topic emis.auth.teacher.created
```

### Step 5: Verify Event Consumed (AuthService Logs)

**AuthService console should show:**

```
info: EMIS.EventBus.Kafka.Kafka.KafkaConsumerService[0]
      Received message from topic emis.auth.teacher.created, partition 3, offset 0

info: Auth.Application.EventHandlers.TeacherCreatedEventHandler[0]
      Handling TeacherCreatedEvent for Teacher 3fa85f64-5717-4562-b3fc-2c963f66afa6 with email nguyenvanb@school.edu.vn

info: Auth.Application.EventHandlers.TeacherCreatedEventHandler[0]
      Successfully created user account for Teacher 3fa85f64-5717-4562-b3fc-2c963f66afa6. Username: nguyenvanb@school.edu.vn
```

### Step 6: Verify Event in Kafka UI

1. Open browser: http://localhost:8080
2. Navigate to **Topics** → **emis.auth.teacher.created**
3. Click **Messages** tab
4. You should see the TeacherCreatedEvent message:

```json
{
  "eventId": "7b2c1234-5678-90ab-cdef-1234567890ab",
  "eventType": "emis.auth.teacher.created",
  "timestamp": "2025-10-26T01:30:00.123Z",
  "teacherId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "Nguyen Van B",
  "email": "nguyenvanb@school.edu.vn",
  "phoneNumber": "0907654321",
  "dateOfBirth": "1988-08-20T00:00:00",
  "defaultPassword": "Teacher@abc12345",
  "subject": null,
  "schoolId": null
}
```

**Message Headers:**
- `EventType`: TeacherCreatedEvent
- `EventId`: 7b2c1234-5678-90ab-cdef-1234567890ab
- `Timestamp`: 2025-10-26T01:30:00.123Z

### Step 7: Verify Account Created in Database

```bash
# Connect to AuthService database
docker exec -it emis-auth-db mysql -uroot -prootpassword EMIS_AuthDB

# Query user table
SELECT Id, Username, Email, FullName, CreatedAt 
FROM Users 
WHERE Email = 'nguyenvanb@school.edu.vn';
```

**Expected output:**
```
+--------------------------------------+---------------------------+---------------------------+---------------+---------------------+
| Id                                   | Username                  | Email                     | FullName      | CreatedAt           |
+--------------------------------------+---------------------------+---------------------------+---------------+---------------------+
| 9c1e2345-6789-0abc-def1-234567890abc | nguyenvanb@school.edu.vn  | nguyenvanb@school.edu.vn  | Nguyen Van B  | 2025-10-26 01:30:01 |
+--------------------------------------+---------------------------+---------------------------+---------------+---------------------+
```

**Verify role:**
```sql
SELECT u.Username, r.Name as RoleName
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'nguyenvanb@school.edu.vn';
```

**Expected output:**
```
+---------------------------+-----------+
| Username                  | RoleName  |
+---------------------------+-----------+
| nguyenvanb@school.edu.vn  | Teacher   |
+---------------------------+-----------+
```

### Step 8: Test Login with Generated Credentials

**Get the default password from TeacherCreatedEvent** (from Kafka UI or AuthService logs)

Example password format: `Teacher@abc12345`

```bash
curl -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "nguyenvanb@school.edu.vn",
    "password": "Teacher@abc12345"
  }'
```

**Or using REST Client:**

```http
### Login with Teacher Account
POST http://localhost:5002/api/auth/login
Content-Type: application/json

{
  "username": "nguyenvanb@school.edu.vn",
  "password": "Teacher@abc12345"
}
```

**Expected HTTP Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "nguyenvanb@school.edu.vn",
  "email": "nguyenvanb@school.edu.vn",
  "fullName": "Nguyen Van B",
  "roles": ["Teacher"]
}
```

### Step 9: Decode JWT Token

Copy the token and decode at https://jwt.io

**Expected claims:**
```json
{
  "sub": "9c1e2345-6789-0abc-def1-234567890abc",
  "username": "nguyenvanb@school.edu.vn",
  "email": "nguyenvanb@school.edu.vn",
  "role": "Teacher",
  "iss": "EMIS.AuthService",
  "aud": "EMIS.Client",
  "exp": 1730000000,
  "iat": 1729996400
}
```

## Testing Error Scenarios

### Test 1: Consumer Offline (Event Queuing)

1. **Stop AuthService** (Ctrl+C in AuthService terminal)
2. **Create teacher** via POST /api/teachers
3. **Verify event in Kafka UI** - message should be queued
4. **Restart AuthService**
5. **Verify logs** - consumer should process queued message
6. **Verify account created** in database

**Expected behavior:** Event persists in Kafka, consumer processes when back online

### Test 2: Duplicate Email

1. Create teacher with email `duplicate@school.edu.vn`
2. Try to create another teacher with same email
3. **Expected:** Second request should fail (business validation)
4. **Verify:** Only one event published, only one account created

### Test 3: Missing Email (Phone as Username)

```http
POST http://localhost:5000/api/teachers
Content-Type: application/json

{
  "firstName": "Tran Thi",
  "lastName": "C",
  "phone": "0912345678",
  "dateOfBirth": "1990-03-10"
}
```

**Expected:**
- Event published with `email: ""`
- AuthService uses `phoneNumber` as username
- Account created with username: `0912345678`

### Test 4: Consumer Error (Account Creation Fails)

1. Manually create user in AuthService DB with email `conflict@school.edu.vn`
2. Create teacher with same email
3. **Expected AuthService logs:**
   ```
   error: Auth.Application.EventHandlers.TeacherCreatedEventHandler[0]
         Failed to create user account for Teacher xxx. Error: Username already exists
   ```
4. **Expected behavior:** 
   - Exception thrown, offset NOT committed
   - Message retried according to Kafka consumer config
   - Message moved to Dead Letter Queue after max retries (future enhancement)

## Monitoring & Troubleshooting

### Check Kafka Consumer Group

```bash
# View consumer group details
docker exec emis-kafka-1 kafka-consumer-groups \
  --bootstrap-server kafka-1:29092 \
  --describe \
  --group emis-auth-service

# Expected output shows lag, current offset, log end offset per partition
```

### Check Topic Message Count

```bash
# Get earliest and latest offsets
docker exec emis-kafka-1 kafka-run-class kafka.tools.GetOffsetShell \
  --broker-list kafka-1:29092 \
  --topic emis.auth.teacher.created \
  --time -1
```

### View Recent Messages (Console Consumer)

```bash
docker exec emis-kafka-1 kafka-console-consumer \
  --bootstrap-server kafka-1:29092 \
  --topic emis.auth.teacher.created \
  --from-beginning \
  --max-messages 5 \
  --property print.timestamp=true \
  --property print.key=true \
  --property print.headers=true
```

### Common Issues

#### Issue: "Broker may not be available"
**Solution:**
```bash
docker compose restart kafka-1 kafka-2 kafka-3
sleep 30  # Wait for cluster to stabilize
```

#### Issue: "Topic does not exist"
**Solution:**
```bash
docker exec emis-kafka-1 kafka-topics --create \
  --bootstrap-server kafka-1:29092,kafka-2:29093,kafka-3:29094 \
  --topic emis.auth.teacher.created \
  --partitions 10 \
  --replication-factor 3 \
  --config min.insync.replicas=2 \
  --config retention.ms=604800000
```

#### Issue: Consumer not receiving messages
**Check:**
1. Consumer group ID matches config: `emis-auth-service`
2. Topic name matches: `emis.auth.teacher.created`
3. Consumer is subscribed: Check AuthService startup logs
4. No consumer lag: Check consumer group details
5. Firewall/network: Ensure ports 9092, 9093, 9094 accessible

## Performance Testing

### Load Test: Create 100 Teachers

```bash
# Using Apache Bench
for i in {1..100}; do
  curl -X POST http://localhost:5000/api/teachers \
    -H "Content-Type: application/json" \
    -d "{
      \"firstName\": \"Teacher\",
      \"lastName\": \"$i\",
      \"email\": \"teacher$i@school.edu.vn\",
      \"phone\": \"090000$i\",
      \"dateOfBirth\": \"1990-01-01\"
    }" &
done
wait
```

**Monitor:**
1. Kafka UI - message throughput
2. AuthService logs - processing rate
3. Database - account creation rate
4. Consumer lag - should return to 0 after processing

**Expected results:**
- All 100 events published successfully
- All 100 accounts created (eventual consistency - may take a few seconds)
- No duplicates
- No data loss

## Success Criteria

✅ Teacher created via API (HTTP 200)  
✅ Event published to Kafka (TeacherService logs)  
✅ Event visible in Kafka UI  
✅ Event consumed by AuthService (AuthService logs)  
✅ Account created in database (SQL query confirms)  
✅ Teacher can login with generated password (HTTP 200, JWT token returned)  
✅ JWT token contains correct role: "Teacher"  
✅ Consumer handles errors gracefully (retry logic works)  
✅ Events persist in Kafka for 7 days (retention policy)  
✅ System handles load (100+ concurrent requests)

## Next Steps

1. ✅ Test ParentCreatedEvent and StudentCreatedEvent flows
2. ✅ Implement Dead Letter Queue (DLQ) for failed messages
3. ✅ Add distributed tracing (OpenTelemetry)
4. ✅ Set up alerts for consumer lag
5. ✅ Implement event versioning strategy
6. ✅ Add integration tests with Testcontainers

## References

- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [EMIS EventBus Architecture](./EVENTBUS_ARCHITECTURE.md)
- [EMIS Contracts Library](../src/Shared/EMIS.Contracts/README.md)
- [Kafka EventBus Implementation](../src/Shared/EMIS.EventBus.Kafka/README.md)
