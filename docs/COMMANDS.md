# Quick Reference - EMIS Commands

## Docker Commands

### Start all infrastructure services
```bash
docker-compose up -d
```

### Stop all services
```bash
docker-compose down
```

### Stop and remove volumes (reset databases)
```bash
docker-compose down -v
```

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f studentdb
docker-compose logs -f rabbitmq
```

### Check running containers
```bash
docker-compose ps
```

### Restart a service
```bash
docker-compose restart studentdb
```

## .NET Commands

### Restore dependencies
```bash
dotnet restore
```

### Build solution
```bash
dotnet build
```

### Build specific project
```bash
dotnet build src/Services/StudentService/Student.API
```

### Run project
```bash
cd src/Services/StudentService/Student.API
dotnet run
```

### Run with watch (hot reload)
```bash
dotnet watch run
```

### Clean build artifacts
```bash
dotnet clean
```

## Entity Framework Commands

### Add migration
```bash
cd src/Services/StudentService/Student.API
dotnet ef migrations add MigrationName --project ../Student.Infrastructure --startup-project .
```

### Update database
```bash
dotnet ef database update --project ../Student.Infrastructure --startup-project .
```

### Remove last migration
```bash
dotnet ef migrations remove --project ../Student.Infrastructure --startup-project .
```

### Generate SQL script
```bash
dotnet ef migrations script --project ../Student.Infrastructure --startup-project .
```

### Drop database
```bash
dotnet ef database drop --project ../Student.Infrastructure --startup-project .
```

### List migrations
```bash
dotnet ef migrations list --project ../Student.Infrastructure --startup-project .
```

## Testing Commands

### Run all tests
```bash
dotnet test
```

### Run tests with detailed output
```bash
dotnet test --verbosity normal
```

### Run tests with coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Run specific test project
```bash
dotnet test tests/Student.UnitTests
```

## Git Commands

### Create feature branch
```bash
git checkout -b feature/your-feature-name
```

### Stage changes
```bash
git add .
```

### Commit changes
```bash
git commit -m "feat: your commit message"
```

### Push to remote
```bash
git push origin feature/your-feature-name
```

### Pull latest changes
```bash
git pull origin develop
```

### View status
```bash
git status
```

### View commit history
```bash
git log --oneline
```

## Database Access

### PostgreSQL (Student DB)
```bash
# Using psql
docker exec -it emis-student-db psql -U postgres -d EMIS_StudentDB

# Using docker-compose
docker-compose exec studentdb psql -U postgres -d EMIS_StudentDB
```

### MongoDB (Messaging DB)
```bash
# Using mongosh
docker exec -it emis-messaging-db mongosh -u admin -p admin

# Using docker-compose
docker-compose exec messagingdb mongosh -u admin -p admin
```

### Redis CLI
```bash
# Access Redis CLI
docker exec -it emis-redis redis-cli

# Check keys
docker exec -it emis-redis redis-cli KEYS '*'
```

## Development Workflow

### 1. Start infrastructure
```bash
docker-compose up -d
```

### 2. Run migrations
```bash
cd src/Services/StudentService/Student.API
dotnet ef database update --project ../Student.Infrastructure --startup-project .
```

### 3. Run service
```bash
dotnet run
# or
dotnet watch run
```

### 4. Access Swagger
```
http://localhost:5001/swagger
```

## API Testing with curl

### Create Student
```bash
curl -X POST http://localhost:5001/api/students \
  -H "Content-Type: application/json" \
  -d '{
    "studentCode": "HS001",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "dateOfBirth": "2018-05-15",
    "gender": 1,
    "enrollmentDate": "2024-09-01"
  }'
```

### Get Student by ID
```bash
curl http://localhost:5001/api/students/{id}
```

### Get All Students
```bash
curl http://localhost:5001/api/students
```

### Health Check
```bash
curl http://localhost:5001/health
```

## Useful VS Code Commands

### Open Command Palette
```
Cmd + Shift + P (macOS)
Ctrl + Shift + P (Windows/Linux)
```

### Run Task
```
Cmd + Shift + B (macOS)
Ctrl + Shift + B (Windows/Linux)
```

### Toggle Terminal
```
Cmd + ` (macOS)
Ctrl + ` (Windows/Linux)
```

## Troubleshooting

### Port already in use
```bash
# Find process using port 5001
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Clear Docker cache
```bash
docker system prune -a
```

### Reset everything
```bash
# Stop all containers
docker-compose down -v

# Remove all .NET build artifacts
dotnet clean
rm -rf **/bin **/obj

# Rebuild
dotnet restore
dotnet build
```

### View container logs
```bash
docker logs emis-student-api
docker logs emis-student-db
```

## Package Management

### Add NuGet package
```bash
dotnet add package PackageName --version X.X.X
```

### Remove NuGet package
```bash
dotnet remove package PackageName
```

### List packages
```bash
dotnet list package
```

### Update packages
```bash
dotnet list package --outdated
dotnet add package PackageName
```

## Code Formatting

### Format code
```bash
dotnet format
```

### Check code style
```bash
dotnet format --verify-no-changes
```

## Performance

### Publish for production
```bash
dotnet publish -c Release -o ./publish
```

### Run in production mode
```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## Monitoring

### Check service health
```bash
curl http://localhost:5001/health
```

### View metrics (if configured)
```bash
curl http://localhost:5001/metrics
```

## RabbitMQ Management

### Access Management UI
```
http://localhost:15672
Username: admin
Password: admin
```

### View queues via CLI
```bash
docker exec -it emis-rabbitmq rabbitmqctl list_queues
```

## Redis Management

### Check Redis connection
```bash
docker exec -it emis-redis redis-cli ping
```

### Monitor Redis commands
```bash
docker exec -it emis-redis redis-cli monitor
```

---

**Pro Tips:**

1. Use `dotnet watch run` for automatic reload during development
2. Keep Docker Desktop running for database services
3. Use Swagger UI for API testing
4. Check logs when something goes wrong
5. Run migrations after pulling new code

**Quick Start (from scratch):**
```bash
docker-compose up -d
cd src/Services/StudentService/Student.API
dotnet ef database update --project ../Student.Infrastructure --startup-project .
dotnet run
# Open http://localhost:5001/swagger
```
