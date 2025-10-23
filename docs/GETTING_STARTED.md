# Getting Started với EMIS Backend

Hướng dẫn này sẽ giúp bạn cài đặt và chạy hệ thống EMIS trên máy local.

## Yêu cầu hệ thống

### Bắt buộc
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - [Download](https://git-scm.com/downloads)

### Khuyến nghị
- **Visual Studio 2022** (hoặc Visual Studio Code với C# extension)
- **Azure Data Studio** hoặc **pgAdmin** (để quản lý PostgreSQL)
- **Postman** (để test APIs)

## Cài đặt

### 1. Clone Repository

```bash
git clone <repository-url>
cd emis-app-backend
```

### 2. Khởi động Infrastructure với Docker

Hệ thống sử dụng Docker Compose để chạy các services cần thiết:

```bash
# Start all infrastructure services
docker-compose up -d

# Kiểm tra status
docker-compose ps

# Xem logs
docker-compose logs -f
```

Services sẽ được khởi động:
- PostgreSQL (Student DB) - Port 5432
- PostgreSQL (Teacher DB) - Port 5433
- PostgreSQL (Attendance DB) - Port 5434
- PostgreSQL (Fee DB) - Port 5435
- MongoDB (Messaging DB) - Port 27017
- Redis - Port 6379
- RabbitMQ - Port 5672, Management UI: 15672

### 3. Restore Dependencies

```bash
# Restore tất cả dependencies
dotnet restore EMIS.sln
```

### 4. Setup Database

#### Option 1: Sử dụng Entity Framework Migrations

```bash
# Navigate to Student.API project
cd src/Services/StudentService/Student.API

# Add migration (nếu chưa có)
dotnet ef migrations add InitialCreate --project ../Student.Infrastructure --startup-project .

# Update database
dotnet ef database update --project ../Student.Infrastructure --startup-project .

# Quay lại root directory
cd ../../../..
```

#### Option 2: Chạy SQL Script

Bạn có thể tạo database manually:

```sql
-- Connect to PostgreSQL
-- Create database nếu chưa có
CREATE DATABASE "EMIS_StudentDB";

-- Run migrations sẽ tự tạo tables
```

### 5. Chạy Services

#### Option A: Chạy với Visual Studio

1. Mở `EMIS.sln` trong Visual Studio 2022
2. Set multiple startup projects:
   - Right-click Solution → Properties
   - Chọn "Multiple startup projects"
   - Set Action = "Start" cho:
     - Student.API
     - (các service khác khi có)
3. Press F5 để chạy

#### Option B: Chạy với Command Line

**Terminal 1 - Student Service:**
```bash
cd src/Services/StudentService/Student.API
dotnet run
```

Service sẽ chạy tại: `http://localhost:5001`

**Terminal 2 - API Gateway (nếu có):**
```bash
cd src/ApiGateway
dotnet run
```

Gateway sẽ chạy tại: `http://localhost:5000`

### 6. Truy cập Swagger UI

Mở browser và truy cập:

- Student Service: http://localhost:5001/swagger
- API Gateway: http://localhost:5000/swagger

## Test API

### Tạo Grade (Khối lớp)

```bash
curl -X POST http://localhost:5001/api/grades \
  -H "Content-Type: application/json" \
  -d '{
    "gradeName": "Lớp 1",
    "gradeCode": "L1",
    "level": 1,
    "description": "Khối lớp 1"
  }'
```

### Tạo School Year

```bash
curl -X POST http://localhost:5001/api/schoolyears \
  -H "Content-Type: application/json" \
  -d '{
    "yearName": "Năm học 2024-2025",
    "yearCode": "2024-2025",
    "startDate": "2024-09-01",
    "endDate": "2025-06-30"
  }'
```

### Tạo Student

```bash
curl -X POST http://localhost:5001/api/students \
  -H "Content-Type: application/json" \
  -d '{
    "studentCode": "HS001",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "dateOfBirth": "2018-05-15",
    "gender": 1,
    "address": "123 Đường ABC, Quận 1, TP.HCM",
    "phone": "0901234567",
    "enrollmentDate": "2024-09-01",
    "bloodType": "O"
  }'
```

### Get Student by ID

```bash
curl -X GET http://localhost:5001/api/students/{id}
```

## Cấu trúc Project

```
emis-app-backend/
├── src/
│   ├── Services/
│   │   └── StudentService/
│   │       ├── Student.Domain/         # Business logic, entities
│   │       ├── Student.Application/    # Use cases, DTOs
│   │       ├── Student.Infrastructure/ # Data access, repositories
│   │       └── Student.API/           # Web API, controllers
│   ├── ApiGateway/                    # Ocelot gateway
│   └── Shared/                        # Shared libraries
├── tests/                             # Unit & Integration tests
├── docs/                              # Documentation
├── docker-compose.yml                 # Docker services
└── EMIS.sln                          # Solution file
```

## Configuration

### Database Connection Strings

Sửa file `appsettings.json` trong mỗi service:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EMIS_StudentDB;Username=postgres;Password=postgres"
  }
}
```

### RabbitMQ Configuration

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "admin",
    "Password": "admin"
  }
}
```

### Redis Configuration

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

## Debugging

### Visual Studio
1. Set breakpoints trong code
2. Press F5 để start debugging
3. Sử dụng Debug toolbar để step through code

### Logs
Logs được output ra Console và có thể configure để ghi vào file:

```bash
# Xem logs của Docker containers
docker-compose logs -f studentdb
docker-compose logs -f rabbitmq
```

## Common Issues & Solutions

### Issue 1: Port đã được sử dụng

**Error**: `Address already in use`

**Solution**:
```bash
# Kiểm tra process đang dùng port
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Issue 2: Database connection failed

**Error**: `Could not connect to database`

**Solution**:
1. Kiểm tra Docker container đang chạy: `docker ps`
2. Kiểm tra connection string trong appsettings.json
3. Kiểm tra PostgreSQL logs: `docker-compose logs studentdb`

### Issue 3: Migration failed

**Error**: `Migration failed`

**Solution**:
```bash
# Drop database và recreate
docker-compose down -v
docker-compose up -d

# Re-run migrations
cd src/Services/StudentService/Student.API
dotnet ef database update --project ../Student.Infrastructure --startup-project .
```

## Development Workflow

### 1. Tạo Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Viết code
- Viết unit tests
- Run tests: `dotnet test`

### 3. Commit & Push

```bash
git add .
git commit -m "feat: add your feature description"
git push origin feature/your-feature-name
```

### 4. Create Pull Request

- Tạo PR trên GitHub/GitLab
- Request review
- Merge sau khi approved

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
cd tests/Student.UnitTests
dotnet test
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

## Next Steps

1. ✅ Bạn đã setup thành công Student Service
2. 📝 Đọc [ARCHITECTURE.md](./ARCHITECTURE.md) để hiểu kiến trúc
3. 🔨 Tìm hiểu thêm về các service khác
4. 💻 Bắt đầu implement features mới

## Support

Nếu bạn gặp vấn đề, vui lòng:
1. Kiểm tra [Common Issues](#common-issues--solutions)
2. Xem logs: `docker-compose logs -f`
3. Tạo issue trên GitHub

---

**Happy Coding! 🚀**
