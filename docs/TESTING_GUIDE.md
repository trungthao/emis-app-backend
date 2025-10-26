# EMIS End-to-End Testing Guide

## 📋 Tổng quan

Hướng dẫn test end-to-end flow của hệ thống Event-Driven Architecture:
```
TeacherService (Publisher) 
    → Kafka (Message Broker) 
        → AuthService (Consumer)
```

## 🎯 Yêu cầu trước khi test

### 1. Infrastructure đang chạy

```bash
docker compose ps
```

Kiểm tra các services sau đang **healthy**:
- ✅ emis-kafka-1, emis-kafka-2, emis-kafka-3
- ✅ emis-zookeeper
- ✅ emis-schema-registry
- ✅ emis-kafka-ui
- ✅ emis-teacher-db (MySQL port 3307)
- ✅ emis-auth-db (MySQL port 3308)

### 2. Kafka Topics đã tạo

```bash
docker exec emis-kafka-1 kafka-topics --list --bootstrap-server kafka-1:29092
```

Các topics cần có:
- ✅ emis.teacher.created
- ✅ emis.class.created
- ✅ emis.class.updated

## 🚀 Cách 1: Test với Script (Nhanh nhất)

### Bước 1: Chạy test guide

```bash
./run-test.sh
```

Script sẽ hiển thị:
- ✅ Infrastructure status
- ✅ Kafka topics và message count
- ✅ Commands để start services
- ✅ Test commands
- ✅ Monitoring commands

### Bước 2: Start Services (2 terminals riêng)

**Terminal 1 - TeacherService:**
```bash
cd /Users/trungthao/Projects/emis-app-backend
dotnet run --project src/Services/TeacherService/Teacher.API/Teacher.API.csproj --urls "http://localhost:5001"
```

**Terminal 2 - AuthService:**
```bash
cd /Users/trungthao/Projects/emis-app-backend
dotnet run --project src/Services/AuthService/Auth.API/Auth.API.csproj --urls "http://localhost:5002"
```

### Bước 3: Test với curl

**Test 1: Create Teacher**
```bash
curl -X POST http://localhost:5001/api/teachers \
  -H "Content-Type: application/json" \
  -d'{
    "fullName": "Nguyễn Văn An",
    "email": "nguyenvanan@school.edu.vn",
    "phoneNumber": "0901234567",
    "dateOfBirth": "1985-05-15",
    "gender": "Male",
    "address": "123 Đường ABC, Quận 1, TP.HCM",
    "hireDate": "2020-09-01",
    "department": "Toán",
    "position": "Giáo viên",
    "qualifications": "Thạc sĩ Toán học",
    "yearsOfExperience": 8,
    "schoolId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Active"
  }'
```

**Expected Response:**
```json
{
  "id": "some-guid",
  "fullName": "Nguyễn Văn An",
  "email": "nguyenvanan@school.edu.vn",
  ...
}
```

**Test 2: Wait & Check Kafka (optional)**
```bash
# Wait for event processing
sleep 3

# Check messages in topic
docker exec emis-kafka-1 kafka-console-consumer \
  --bootstrap-server kafka-1:29092 \
  --topic emis.teacher.created \
  --from-beginning \
  --max-messages 1
```

**Test 3: Login với auto-created account**
```bash
curl -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d'{
    "username": "nguyenvanan@school.edu.vn",
    "password": "Teacher@123"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-27T...",
  "user": {
    "id": "...",
    "username": "nguyenvanan@school.edu.vn",
    "role": "Teacher"
  }
}
```

## 🎯 Cách 2: Test với VS Code REST Client

### Bước 1: Mở file test

```
End-to-End-Test.http
```

### Bước 2: Click "Send Request"

VS Code sẽ hiển thị button "Send Request" phía trên mỗi HTTP request.

Click lần lượt:
1. **Step 1: Create Teacher**
2. Wait 2-3 giây
3. **Step 3: Login**
4. **Step 4: Verify - Get all teachers**

## 📊 Monitoring & Debugging

### Option 1: Kafka UI (Visual - Recommended)

Mở browser:
```
http://localhost:8080
```

Navigate:
1. **Topics** → Click `emis.teacher.created`
2. **Messages** → Xem event đã publish
3. **Consumers** → Xem AuthService consumer status
4. **Consumer Groups** → Check lag

### Option 2: CLI Commands

**1. Count messages in topic:**
```bash
docker exec emis-kafka-1 kafka-run-class kafka.tools.GetOffsetShell \
  --bootstrap-server kafka-1:29092 \
  --topic emis.teacher.created \
  --time -1 | awk -F: '{sum += $3} END {print "Total: " sum}'
```

**2. Read messages:**
```bash
docker exec emis-kafka-1 kafka-console-consumer \
  --bootstrap-server kafka-1:29092 \
  --topic emis.teacher.created \
  --from-beginning \
  --property print.key=true \
  --property print.timestamp=true \
  --max-messages 10
```

**3. Check consumer groups:**
```bash
docker exec emis-kafka-1 kafka-consumer-groups \
  --bootstrap-server kafka-1:29092 \
  --list
```

**4. Check consumer lag:**
```bash
docker exec emis-kafka-1 kafka-consumer-groups \
  --bootstrap-server kafka-1:29092 \
  --group teacher-service-group \
  --describe
```

**5. Check service logs:**

TeacherService Terminal:
```
Look for:
✅ "Publishing event: TeacherCreatedEvent"
✅ "Event published successfully to topic emis.teacher.created"
```

AuthService Terminal:
```
Look for:
✅ "Event received: TeacherCreatedEvent"
✅ "Creating authentication account for: nguyenvanan@school.edu.vn"
✅ "Account created successfully"
```

## 🎯 Expected End-to-End Flow

```
┌──────────────────────────────────────────────────────┐
│  1. POST /api/teachers                               │
│     → TeacherService                                 │
│     → Save to TeacherDB (MySQL 3307)                │
│     → Commit transaction                             │
├──────────────────────────────────────────────────────┤
│  2. Publish Event                                    │
│     → Create TeacherCreatedEvent                     │
│     → Serialize to JSON                              │
│     → Send to Kafka topic: emis.teacher.created     │
├──────────────────────────────────────────────────────┤
│  3. Kafka Broker                                     │
│     → Store event in partition (based on key)       │
│     → Replicate to 3 brokers                        │
│     → Confirm write (min.insync.replicas=2)         │
├──────────────────────────────────────────────────────┤
│  4. AuthService Consumer                             │
│     → Poll Kafka for new messages                   │
│     → Deserialize TeacherCreatedEvent               │
│     → Call TeacherCreatedEventHandler               │
├──────────────────────────────────────────────────────┤
│  5. Create Account                                   │
│     → Generate username (email)                     │
│     → Generate password (Teacher@123)               │
│     → Hash password                                 │
│     → Save to AuthDB (MySQL 3308)                   │
│     → Commit offset to Kafka                        │
├──────────────────────────────────────────────────────┤
│  6. POST /api/auth/login                            │
│     → AuthService validates credentials             │
│     → Generate JWT token                            │
│     → Return token ✅                               │
└──────────────────────────────────────────────────────┘
```

## 🐛 Troubleshooting

### Problem: "Connection refused" khi gọi API

**Solution:**
```bash
# Check service đang chạy
lsof -i :5001  # TeacherService
lsof -i :5002  # AuthService

# Restart service
# Ctrl+C trong terminal, sau đó chạy lại dotnet run
```

### Problem: "Topic not found"

**Solution:**
```bash
# List topics
docker exec emis-kafka-1 kafka-topics --list --bootstrap-server kafka-1:29092

# Create missing topic
docker exec emis-kafka-1 kafka-topics --create \
  --bootstrap-server kafka-1:29092 \
  --topic emis.teacher.created \
  --partitions 10 \
  --replication-factor 3
```

### Problem: Event published nhưng AuthService không consume

**Check 1: Consumer group có đang chạy?**
```bash
docker exec emis-kafka-1 kafka-consumer-groups \
  --bootstrap-server kafka-1:29092 \
  --list
```

**Check 2: Consumer lag?**
```bash
docker exec emis-kafka-1 kafka-consumer-groups \
  --bootstrap-server kafka-1:29092 \
  --group teacher-service-group \
  --describe
```

**Check 3: AuthService logs có errors?**
Xem terminal AuthService, tìm errors liên quan đến:
- Kafka connection
- Event deserialization
- Database errors

### Problem: Login failed với "Invalid credentials"

**Possible causes:**
1. Event chưa được consume (chờ 2-3s)
2. Account chưa được tạo (check AuthService logs)
3. Password sai (default: `Teacher@123`)

**Verify account created:**
```bash
# Connect to AuthDB
docker exec -it emis-auth-db mysql -uroot -proot123 authdb

# Check accounts table
SELECT * FROM Accounts WHERE Username = 'nguyenvanan@school.edu.vn';
```

## 📈 Performance Testing

### Test nhiều teachers cùng lúc

```bash
# Create 10 teachers
for i in {1..10}; do
  curl -X POST http://localhost:5001/api/teachers \
    -H "Content-Type: application/json" \
    -d"{
      \"fullName\": \"Teacher $i\",
      \"email\": \"teacher$i@school.edu.vn\",
      \"phoneNumber\": \"090000000$i\",
      \"dateOfBirth\": \"1985-05-15\",
      \"gender\": \"Male\",
      \"hireDate\": \"2020-09-01\",
      \"department\": \"Math\",
      \"position\": \"Teacher\",
      \"schoolId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",
      \"status\": \"Active\"
    }" &
done

wait

# Check message count
docker exec emis-kafka-1 kafka-run-class kafka.tools.GetOffsetShell \
  --bootstrap-server kafka-1:29092 \
  --topic emis.teacher.created \
  --time -1 | awk -F: '{sum += $3} END {print "Total: " sum}'
```

## ✅ Success Criteria

Test thành công khi:

1. ✅ TeacherService nhận request và return 200 OK
2. ✅ Event xuất hiện trong Kafka topic `emis.teacher.created`
3. ✅ AuthService logs show "Account created successfully"
4. ✅ Login với credentials auto-created return JWT token
5. ✅ No errors in service logs
6. ✅ Consumer lag = 0

---

**Ready to test!** 🚀

Nếu gặp vấn đề, check:
1. Service logs trong terminals
2. Kafka UI: http://localhost:8080
3. Database connections
4. Network ports (5001, 5002)
