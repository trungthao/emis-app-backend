# EMIS - Education Management Information System

Hệ thống quản lý thông tin giáo dục được xây dựng theo kiến trúc Microservices và Clean Architecture với .NET 8.

## 📋 Tổng quan

EMIS là một hệ thống backend toàn diện giúp quản lý các hoạt động trong trường học, bao gồm:
- Quản lý hồ sơ học sinh và phụ huynh
- Quản lý giáo viên và phân công giảng dạy
- Điểm danh và chấm công
- Quản lý thu phí
- Hệ thống nhắn tin giữa giáo viên và phụ huynh

## 🏗️ Kiến trúc hệ thống

### Microservices Architecture

Hệ thống được chia thành các microservices độc lập:

1. **Student Service** - Quản lý học sinh, phụ huynh, lớp học
2. **Teacher Service** - Quản lý giáo viên, phân công giảng dạy
3. **Attendance Service** - Điểm danh học sinh, chấm công giáo viên
4. **Fee Management Service** - Quản lý các khoản thu phí
5. **Messaging Service** - Hệ thống tin nhắn real-time
6. **API Gateway** - Điểm truy cập duy nhất cho tất cả các service

### Clean Architecture

Mỗi microservice tuân thủ Clean Architecture với 4 layers:

```
├── Domain Layer         # Entities, Value Objects, Domain Events
├── Application Layer    # Use Cases, DTOs, Interfaces
├── Infrastructure Layer # Database, External Services, Implementation
└── API Layer           # Controllers, Middleware, Configuration
```

## 📁 Cấu trúc thư mục

```
emis-app-backend/
├── src/
│   ├── ApiGateway/                    # Ocelot API Gateway
│   ├── Services/
│   │   ├── StudentService/
│   │   │   ├── Student.Domain/
│   │   │   ├── Student.Application/
│   │   │   ├── Student.Infrastructure/
│   │   │   └── Student.API/
│   │   ├── TeacherService/
│   │   │   ├── Teacher.Domain/
│   │   │   ├── Teacher.Application/
│   │   │   ├── Teacher.Infrastructure/
│   │   │   └── Teacher.API/
│   │   ├── AttendanceService/
│   │   │   ├── Attendance.Domain/
│   │   │   ├── Attendance.Application/
│   │   │   ├── Attendance.Infrastructure/
│   │   │   └── Attendance.API/
│   │   ├── FeeManagementService/
│   │   │   ├── FeeManagement.Domain/
│   │   │   ├── FeeManagement.Application/
│   │   │   ├── FeeManagement.Infrastructure/
│   │   │   └── FeeManagement.API/
│   │   └── MessagingService/
│   │       ├── Messaging.Domain/
│   │       ├── Messaging.Application/
│   │       ├── Messaging.Infrastructure/
│   │       └── Messaging.API/
│   └── Shared/
│       ├── EMIS.Common/              # Common utilities, extensions
│       ├── EMIS.EventBus/            # Event bus implementation (RabbitMQ/MassTransit)
│       └── EMIS.Authentication/      # JWT Authentication & Authorization
├── tests/                             # Unit & Integration tests
├── docs/                              # Documentation
├── docker-compose.yml                 # Docker compose configuration
└── EMIS.sln                          # Solution file
```

## 🚀 Công nghệ sử dụng

- **.NET 8** - Framework chính
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database cho các service
- **MongoDB** - Database cho Messaging Service
- **Redis** - Caching & Session
- **RabbitMQ** - Message broker
- **MassTransit** - Distributed application framework
- **Ocelot** - API Gateway
- **SignalR** - Real-time communication
- **JWT** - Authentication
- **MediatR** - CQRS pattern
- **FluentValidation** - Validation
- **AutoMapper** - Object mapping
- **Serilog** - Logging
- **Docker** - Containerization

## 📦 Cài đặt và Chạy

### Yêu cầu

- .NET 8 SDK
- Docker & Docker Compose
- Visual Studio 2022 hoặc VS Code

### Chạy với Docker Compose

```bash
# Clone repository
git clone <repository-url>
cd emis-app-backend

# Chạy tất cả services
docker-compose up -d

# Xem logs
docker-compose logs -f

# Dừng services
docker-compose down
```

### Chạy từng service riêng lẻ

```bash
# Restore dependencies
dotnet restore

# Chạy Student Service
cd src/Services/StudentService/Student.API
dotnet run

# Chạy API Gateway
cd src/ApiGateway
dotnet run
```

## 🔧 Cấu hình

Mỗi service có file `appsettings.json` riêng để cấu hình:

- Database connection strings
- JWT settings
- RabbitMQ connection
- Redis connection
- Logging configuration

## 📚 API Documentation

Sau khi chạy, bạn có thể truy cập Swagger UI tại:

- API Gateway: `http://localhost:5000/swagger`
- Student Service: `http://localhost:5001/swagger`
- Teacher Service: `http://localhost:5002/swagger`
- Attendance Service: `http://localhost:5003/swagger`
- Fee Management Service: `http://localhost:5004/swagger`
- Messaging Service: `http://localhost:5005/swagger`

## 🧪 Testing

```bash
# Chạy tất cả tests
dotnet test

# Chạy tests cho một service cụ thể
dotnet test tests/StudentService.Tests
```

## 🔐 Bảo mật

- JWT-based authentication
- Role-based authorization (Admin, Teacher, Parent)
- API rate limiting
- CORS configuration
- HTTPS enforcement

## 📈 Monitoring & Logging

- Structured logging với Serilog
- Log aggregation
- Health checks
- Performance monitoring

## 🤝 Đóng góp

1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📝 License

This project is licensed under the MIT License.

## 👥 Team

- Developer: Your Team
- Contact: your-email@example.com

## 🗺️ Roadmap

- [x] Phase 1: Core Services Implementation
  - [x] Project structure setup
  - [x] Student Service (60% - Core features working)
  - [ ] Teacher Service
  - [ ] Attendance Service
  - [ ] Fee Management Service
  - [ ] Messaging Service
- [ ] Phase 2: Advanced Features
  - [ ] Real-time notifications
  - [ ] Report generation
  - [ ] Mobile app integration
  - [ ] AI-powered analytics
- [ ] Phase 3: Scaling & Optimization
  - [ ] Kubernetes deployment
  - [ ] Performance optimization
  - [ ] Advanced monitoring

📖 **Chi tiết**: Xem [ROADMAP.md](./docs/ROADMAP.md)

## 📞 Support

Nếu bạn có bất kỳ câu hỏi nào, vui lòng:
1. Kiểm tra [Documentation](./docs/)
2. Đọc [Getting Started Guide](./docs/GETTING_STARTED.md)
3. Xem [Quick Commands](./docs/COMMANDS.md)
4. Tạo issue trên GitHub

## 📖 Tài Liệu Bổ Sung

- 📘 [ARCHITECTURE.md](./docs/ARCHITECTURE.md) - Kiến trúc hệ thống chi tiết
- 🚀 [GETTING_STARTED.md](./docs/GETTING_STARTED.md) - Hướng dẫn bắt đầu
- 🗺️ [ROADMAP.md](./docs/ROADMAP.md) - Lộ trình phát triển
- ⚡ [COMMANDS.md](./docs/COMMANDS.md) - Quick reference commands
- 📊 [PROJECT_SUMMARY.md](./docs/PROJECT_SUMMARY.md) - Tổng kết dự án
