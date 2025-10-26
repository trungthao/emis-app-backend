# VS Code Debugging Guide

## 🎯 Quick Start

### Option 1: Debug Single Service (TeacherService)

1. **Mở VS Code** ở workspace root
2. Nhấn **F5** hoặc **Run → Start Debugging**
3. Chọn **"Debug TeacherService"**
4. Service sẽ start ở `http://localhost:5001`
5. Swagger UI tự động mở: `http://localhost:5001/swagger`

### Option 2: Debug Multiple Services (End-to-End)

1. Nhấn **F5**
2. Chọn **"Debug Teacher + Auth (E2E Test)"**
3. Cả TeacherService (5001) và AuthService (5002) sẽ start
4. Bây giờ có thể test end-to-end flow với breakpoints!

### Option 3: Debug All Services

1. Nhấn **F5**
2. Chọn **"Debug All Services"**
3. StudentService, TeacherService, AuthService đều start

---

## 🔧 Debug Configurations

### Available Configurations

| Configuration | Service | Port | Description |
|--------------|---------|------|-------------|
| **Debug TeacherService** | TeacherService | 5001 | Debug publisher service |
| **Debug AuthService** | AuthService | 5002 | Debug consumer service |
| **Debug StudentService** | StudentService | 5003 | Debug student service |
| **Debug Teacher + Auth (E2E Test)** | Teacher + Auth | 5001, 5002 | Test EventBus flow |
| **Debug All Services** | All | 5001, 5002, 5003 | Full microservices |

---

## 🐛 Debugging Workflow

### Step 1: Set Breakpoints

**TeacherService - Publisher:**
```
📍 CreateTeacherCommandHandler.cs, line ~85
   await _eventBus.PublishAsync(teacherCreatedEvent, ...);
```

**AuthService - Consumer:**
```
📍 TeacherCreatedEventHandler.cs, line ~25
   public async Task HandleAsync(TeacherCreatedEvent @event, ...)
```

### Step 2: Start Debugging

1. Press **F5**
2. Select configuration (e.g., "Debug Teacher + Auth (E2E Test)")
3. Wait for services to start (watch Debug Console)

### Step 3: Send Test Request

**Option A: Use REST Client**
- Open `End-to-End-Test.http`
- Click "Send Request" on Step 1 (Create Teacher)

**Option B: Use curl**
```bash
curl -X POST http://localhost:5001/api/teachers \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test Teacher",
    "email": "test@school.edu.vn",
    "phoneNumber": "0901234567",
    "dateOfBirth": "1985-05-15",
    "gender": "Male",
    "hireDate": "2020-09-01",
    "department": "Math",
    "position": "Teacher",
    "schoolId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Active"
  }'
```

### Step 4: Follow Execution

1. **Breakpoint hits** in CreateTeacherCommandHandler
2. Press **F10** (Step Over) or **F11** (Step Into)
3. Watch Variables panel to see event data
4. **Continue** (F5) to publish event
5. **Breakpoint hits** in TeacherCreatedEventHandler (AuthService)
6. Inspect `@event` object
7. Continue to see account created

---

## 📊 Debug Console

### What to Look For

**TeacherService Console:**
```
info: EMIS.EventBus.Kafka.KafkaEventBus[0]
      Publishing event: TeacherCreatedEvent to topic emis.teacher.created

info: EMIS.EventBus.Kafka.KafkaEventBus[0]
      Event published successfully
```

**AuthService Console:**
```
info: EMIS.EventBus.Kafka.Kafka.KafkaConsumerService[0]
      Event received: TeacherCreatedEvent

info: Auth.Application.EventHandlers.TeacherCreatedEventHandler[0]
      Creating authentication account for: test@school.edu.vn

info: Auth.Application.EventHandlers.TeacherCreatedEventHandler[0]
      Account created successfully
```

---

## 🔍 Useful Debug Features

### Variables Panel

Xem real-time values:
- `@event` object
- `teacher` entity
- `account` entity
- Database context state

### Watch Panel

Add expressions:
```csharp
@event.TeacherId
@event.Email
teacher.Id
_unitOfWork.HasChanges()
```

### Call Stack

Trace execution path:
```
TeacherCreatedEventHandler.HandleAsync()
  ↓
KafkaConsumerService.ConsumeAsync()
  ↓
Consumer.Consume()
```

### Debug Console (REPL)

Execute code while paused:
```csharp
> @event.FullName
"Test Teacher"

> teacher.Email
"test@school.edu.vn"

> await _repository.GetByIdAsync(teacher.Id)
Teacher { Id = ..., FullName = "Test Teacher" }
```

---

## 🛠️ Troubleshooting

### Problem: "Cannot connect to runtime process"

**Solution:**
```bash
# Install C# extension
code --install-extension ms-dotnettools.csharp

# Restart VS Code
```

### Problem: Breakpoint shows "Not yet bound"

**Reasons:**
1. Code not built → Run build task first
2. PDB files missing → Clean and rebuild
3. Code changed → Restart debugging

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet build src/Services/TeacherService/Teacher.API/Teacher.API.csproj
```

### Problem: Port already in use

**Solution:**
```bash
# Find process using port
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Problem: Kafka errors when starting

**Check:**
```bash
# Ensure Kafka is running
docker ps | grep kafka

# Check topics exist
docker exec emis-kafka-1 kafka-topics --list --bootstrap-server kafka-1:29092
```

---

## 📝 Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| **F5** | Start Debugging / Continue |
| **Shift+F5** | Stop Debugging |
| **Ctrl+Shift+F5** | Restart Debugging |
| **F9** | Toggle Breakpoint |
| **F10** | Step Over |
| **F11** | Step Into |
| **Shift+F11** | Step Out |
| **Ctrl+K Ctrl+I** | Show Hover Info |

---

## 🎯 Common Debug Scenarios

### Scenario 1: Debug Event Publishing

**Goal:** Verify event is published correctly

**Steps:**
1. Set breakpoint in `CreateTeacherCommandHandler.cs` line 85
2. Start "Debug TeacherService"
3. Send POST request
4. When breakpoint hits, inspect `teacherCreatedEvent`:
   ```csharp
   EventType: "emis.teacher.created"
   TeacherId: <guid>
   Email: "test@school.edu.vn"
   ```
5. Press F10 to step through `PublishAsync`
6. Watch Debug Console for "Event published successfully"

### Scenario 2: Debug Event Consumption

**Goal:** Verify event is consumed and account created

**Steps:**
1. Set breakpoint in `TeacherCreatedEventHandler.cs` line 25
2. Start "Debug Teacher + Auth (E2E Test)"
3. Send POST request to TeacherService
4. Breakpoint hits in AuthService
5. Inspect `@event` parameter
6. Step through account creation logic
7. Verify account in database

### Scenario 3: Debug Database Transactions

**Goal:** See what's saved to database

**Steps:**
1. Set breakpoint BEFORE `await _unitOfWork.SaveChangesAsync()`
2. Open Watch panel, add:
   ```
   _dbContext.ChangeTracker.Entries()
   ```
3. See entities being tracked (Added, Modified, Deleted)
4. Step over SaveChangesAsync
5. Verify transaction committed

---

## 🔥 Hot Reload

### Enable Hot Reload while Debugging

VS Code supports .NET Hot Reload! Changes take effect without restart.

**To use:**
1. Start debugging (F5)
2. Make code changes
3. Save file (Cmd+S)
4. Changes apply automatically (for supported changes)

**Supported changes:**
- ✅ Method body changes
- ✅ Adding new methods
- ✅ Changing strings, constants
- ❌ Changing signatures
- ❌ Adding new dependencies

---

## 📊 Performance Profiling

### CPU Profiling

```bash
# Install dotnet-trace
dotnet tool install --global dotnet-trace

# Start service
dotnet run --project src/Services/TeacherService/Teacher.API

# Collect trace (in another terminal)
dotnet trace collect --process-id <PID> --duration 00:00:30

# Analyze trace file
# Open .nettrace file in VS Code or Visual Studio
```

### Memory Profiling

```bash
# Install dotnet-counters
dotnet tool install --global dotnet-counters

# Monitor memory
dotnet counters monitor --process-id <PID> \
  --counters System.Runtime[gc-heap-size,gen-0-gc-count,gen-1-gc-count]
```

---

## ✅ Best Practices

1. **Use Conditional Breakpoints**
   - Right-click breakpoint → Edit Breakpoint
   - Add condition: `@event.Email == "specific@email.com"`

2. **Use Logpoints**
   - Right-click → Add Logpoint
   - Message: `Teacher created: {@event.FullName}`
   - No code changes needed!

3. **Debug Multiple Services Together**
   - Use "Debug Teacher + Auth (E2E Test)"
   - See full flow from publish → consume

4. **Watch Kafka Messages**
   - Keep Kafka UI open: http://localhost:8080
   - See messages in real-time

5. **Use Debug Console REPL**
   - Execute C# expressions while paused
   - Query database, inspect objects

---

**Happy Debugging!** 🐛🔍
