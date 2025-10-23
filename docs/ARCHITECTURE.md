# Kiến trúc Hệ thống EMIS

## 1. Tổng quan Kiến trúc

EMIS được xây dựng theo mô hình **Microservices Architecture** kết hợp với **Clean Architecture** cho mỗi service. Hệ thống được thiết kế để đảm bảo:

- **Scalability**: Khả năng mở rộng cao
- **Maintainability**: Dễ bảo trì và phát triển
- **Testability**: Dễ dàng test
- **Independence**: Các service độc lập với nhau
- **Resilience**: Khả năng chịu lỗi tốt

## 2. Microservices

### 2.1. Student Service
**Chức năng**:
- Quản lý hồ sơ học sinh
- Quản lý thông tin phụ huynh
- Quản lý lớp học, khối lớp, năm học
- Phân lớp cho học sinh

**Database**: PostgreSQL
**Port**: 5001
**Entities**:
- StudentEntity
- Parent
- ParentStudent (Many-to-Many relationship)
- Class
- ClassStudent (Class history)
- Grade
- SchoolYear

### 2.2. Teacher Service
**Chức năng**:
- Quản lý hồ sơ giáo viên
- Quản lý môn học
- Phân công giáo viên phụ trách lớp
- Phân công giảng dạy môn học

**Database**: PostgreSQL
**Port**: 5002
**Entities**:
- Teacher
- Subject
- TeacherSubject
- ClassTeacher
- TeachingAssignment

### 2.3. Attendance Service
**Chức năng**:
- Điểm danh học sinh hàng ngày
- Chấm công giáo viên
- Nhận xét hoạt động hàng ngày của học sinh
- Báo cáo thống kê điểm danh

**Database**: PostgreSQL
**Port**: 5003
**Entities**:
- StudentAttendance
- TeacherAttendance
- DailyActivity
- AttendanceReport

### 2.4. Fee Management Service
**Chức năng**:
- Quản lý các loại khoản thu
- Quản lý học phí, ăn uống, đồng phục, etc.
- Quản lý thanh toán
- Báo cáo công nợ

**Database**: PostgreSQL
**Port**: 5004
**Entities**:
- FeeType
- FeeSchedule
- StudentFee
- Payment
- Invoice

### 2.5. Messaging Service
**Chức năng**:
- Tin nhắn real-time giữa giáo viên và phụ huynh
- Thông báo hệ thống
- Lịch sử tin nhắn
- File attachments

**Database**: MongoDB (phù hợp cho chat messages)
**Port**: 5005
**Technologies**: SignalR for real-time communication
**Collections**:
- Conversations
- Messages
- Notifications

### 2.6. API Gateway
**Chức năng**:
- Điểm truy cập duy nhất cho tất cả services
- Routing requests
- Authentication & Authorization
- Rate limiting
- Load balancing

**Technology**: Ocelot
**Port**: 5000

## 3. Clean Architecture trong mỗi Service

Mỗi microservice tuân thủ Clean Architecture với 4 layers:

```
┌─────────────────────────────────────────────────────┐
│                   API Layer (Web)                   │
│  - Controllers                                      │
│  - Middleware                                       │
│  - Filters                                          │
│  - Configuration                                    │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│              Application Layer                      │
│  - Use Cases (Commands/Queries)                    │
│  - DTOs                                            │
│  - Validators                                       │
│  - Interfaces                                       │
│  - MediatR Handlers                                │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│               Domain Layer                          │
│  - Entities                                        │
│  - Value Objects                                    │
│  - Domain Events                                    │
│  - Repository Interfaces                            │
│  - Business Logic                                   │
└────────────────┬────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────┐
│            Infrastructure Layer                     │
│  - DbContext                                       │
│  - Repository Implementations                       │
│  - External Services                                │
│  - Database Migrations                              │
└─────────────────────────────────────────────────────┘
```

### 3.1. Dependency Rule
Các dependencies chỉ đi vào trong (inward):
- API → Application → Domain
- Infrastructure → Application, Domain

**Domain Layer** không phụ thuộc vào bất kỳ layer nào khác.

### 3.2. Patterns được sử dụng

#### CQRS (Command Query Responsibility Segregation)
- **Commands**: Thay đổi state (Create, Update, Delete)
- **Queries**: Đọc data (Get, List, Search)
- Implement với **MediatR**

#### Repository Pattern
- Abstraction của data access layer
- Interface định nghĩa trong Domain
- Implementation trong Infrastructure

#### Unit of Work Pattern
- Quản lý transactions
- Đảm bảo consistency khi thao tác nhiều repositories

#### Domain Events
- Publish events khi có thay đổi quan trọng
- Decouple các module trong hệ thống

## 4. Communication giữa Services

### 4.1. Synchronous Communication
- **REST API**: Gọi trực tiếp giữa các services khi cần
- **gRPC**: Cho các tương tác performance-critical (future implementation)

### 4.2. Asynchronous Communication
- **Message Broker**: RabbitMQ + MassTransit
- **Event Bus Pattern**: Publish/Subscribe
- **Use Cases**:
  - Student created → Notify messaging service
  - Class assigned → Update attendance service
  - Fee payment → Notify parent via messaging

### 4.3. Event Examples
```csharp
// Domain Events
StudentCreatedEvent
StudentAssignedToClassEvent
AttendanceMarkedEvent
FeePaymentReceivedEvent
MessageSentEvent
```

## 5. Data Storage

### 5.1. Database per Service
Mỗi service có database riêng để đảm bảo independence:

| Service | Database | Port |
|---------|----------|------|
| Student | PostgreSQL | 5432 |
| Teacher | PostgreSQL | 5433 |
| Attendance | PostgreSQL | 5434 |
| Fee Management | PostgreSQL | 5435 |
| Messaging | MongoDB | 27017 |

### 5.2. Caching
- **Redis**: Distributed cache
- Cache frequently accessed data:
  - Current school year
  - Active classes
  - Student basic info
  - Permission/Role cache

### 5.3. Data Consistency
- **Eventual Consistency**: Chấp nhận cho các operations không critical
- **Saga Pattern**: Cho distributed transactions
- **Outbox Pattern**: Đảm bảo delivery của events

## 6. Security

### 6.1. Authentication
- **JWT (JSON Web Token)**
- Shared secret key across services
- Token includes: UserId, Role, Claims

### 6.2. Authorization
**Roles**:
- **Admin**: Full access
- **Teacher**: Manage classes, students, attendance
- **Parent**: View own children info, messaging

**Permissions**: Role-based access control (RBAC)

### 6.3. API Gateway Security
- Rate limiting
- IP whitelisting (optional)
- HTTPS enforcement
- CORS configuration

## 7. Monitoring & Logging

### 7.1. Structured Logging
- **Serilog**: Structured logging framework
- Log levels: Debug, Information, Warning, Error, Fatal
- Correlation IDs for tracing requests across services

### 7.2. Health Checks
Each service exposes `/health` endpoint:
- Database connectivity
- External services status
- Memory usage

### 7.3. Metrics (Future)
- **Prometheus + Grafana**
- Application metrics
- Infrastructure metrics
- Business metrics

## 8. Deployment

### 8.1. Containerization
- **Docker**: Containerize all services
- **Docker Compose**: Local development
- Each service has its own Dockerfile

### 8.2. Orchestration (Future)
- **Kubernetes**: Production deployment
- Auto-scaling
- Service discovery
- Load balancing

### 8.3. CI/CD Pipeline (Future)
- GitHub Actions / Azure DevOps
- Automated testing
- Automated deployment
- Blue-green deployment

## 9. Development Workflow

### 9.1. Branch Strategy
- `main`: Production code
- `develop`: Development branch
- `feature/*`: Feature branches
- `hotfix/*`: Hotfix branches

### 9.2. Code Review
- Pull Request required
- At least 1 approval
- All tests must pass

### 9.3. Testing Strategy
- **Unit Tests**: Domain & Application logic
- **Integration Tests**: API endpoints
- **E2E Tests**: Complete workflows
- **Load Tests**: Performance testing

## 10. Scalability Considerations

### 10.1. Horizontal Scaling
- Stateless services
- Load balancer distributes requests
- Database read replicas

### 10.2. Performance Optimization
- Caching strategy
- Database indexing
- Query optimization
- Async operations where possible

### 10.3. Future Enhancements
- Event Sourcing for audit trail
- CQRS with separate read/write databases
- API versioning
- GraphQL API (optional)
- Mobile app backend support

## 11. Error Handling

### 11.1. Global Exception Handler
- Centralized error handling
- Consistent error response format
- Error logging

### 11.2. Validation
- FluentValidation at Application layer
- Model validation at API layer
- Business rules validation in Domain

### 11.3. Resilience
- Retry policies with Polly
- Circuit breaker pattern
- Fallback strategies

## 12. Documentation

### 12.1. API Documentation
- **Swagger/OpenAPI**: Interactive API docs
- Each service has its own Swagger UI
- API Gateway aggregates all docs

### 12.2. Code Documentation
- XML comments for public APIs
- README for each service
- Architecture Decision Records (ADRs)

---

## Conclusion

Kiến trúc EMIS được thiết kế để đáp ứng các yêu cầu:
- ✅ Dễ maintain và extend
- ✅ Scale independently
- ✅ High availability
- ✅ Security & performance
- ✅ Developer-friendly

Hệ thống có thể phát triển thêm các services mới mà không ảnh hưởng đến services hiện tại.
