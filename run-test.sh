#!/bin/bash

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  EMIS End-to-End Test Guide${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Step 1: Check infrastructure
echo -e "${YELLOW}Step 1: Checking Infrastructure...${NC}"
echo ""
echo "Kafka cluster:"
docker ps --filter "name=kafka" --format "  {{.Names}}: {{.Status}}"
echo ""
echo "Databases:"
docker ps --filter "name=db" --format "  {{.Names}}: {{.Status}}"
echo ""

# Step 2: Check topics
echo -e "${YELLOW}Step 2: Checking Kafka Topics...${NC}"
echo ""
docker exec emis-kafka-1 kafka-topics --list --bootstrap-server kafka-1:29092 | grep "emis\." | while read topic; do
    count=$(docker exec emis-kafka-1 kafka-run-class kafka.tools.GetOffsetShell \
        --bootstrap-server kafka-1:29092 \
        --topic $topic \
        --time -1 2>/dev/null | awk -F: '{sum += $3} END {print sum}')
    echo "  $topic: $count messages"
done
echo ""

# Step 3: Instructions
echo -e "${YELLOW}Step 3: Start Services (Run in separate terminals)${NC}"
echo ""
echo -e "${GREEN}Terminal 1 - TeacherService:${NC}"
echo "  cd /Users/trungthao/Projects/emis-app-backend"
echo "  dotnet run --project src/Services/TeacherService/Teacher.API/Teacher.API.csproj --urls \"http://localhost:5001\""
echo ""
echo -e "${GREEN}Terminal 2 - AuthService:${NC}"
echo "  cd /Users/trungthao/Projects/emis-app-backend"
echo "  dotnet run --project src/Services/AuthService/Auth.API/Auth.API.csproj --urls \"http://localhost:5002\""
echo ""

# Step 4: Test instructions
echo -e "${YELLOW}Step 4: Run Tests${NC}"
echo ""
echo "Option 1: Use VS Code REST Client"
echo "  - Open file: End-to-End-Test.http"
echo "  - Click 'Send Request' above each request"
echo ""
echo "Option 2: Use curl commands"
echo ""
echo -e "${GREEN}Test 1: Create Teacher${NC}"
echo 'curl -X POST http://localhost:5001/api/teachers \
  -H "Content-Type: application/json" \
  -d'"'"'{
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
  }'"'"
echo ""
echo ""
echo -e "${GREEN}Test 2: Wait 2-3 seconds, then Login${NC}"
echo 'sleep 3'
echo 'curl -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d'"'"'{
    "username": "nguyenvanan@school.edu.vn",
    "password": "Teacher@123"
  }'"'"
echo ""
echo ""

# Step 5: Monitoring
echo -e "${YELLOW}Step 5: Monitoring${NC}"
echo ""
echo "Kafka UI (Visual): http://localhost:8080"
echo ""
echo "Check messages in topic:"
echo '  docker exec emis-kafka-1 kafka-console-consumer \
    --bootstrap-server kafka-1:29092 \
    --topic emis.teacher.created \
    --from-beginning \
    --max-messages 5'
echo ""
echo "Check consumer lag:"
echo '  docker exec emis-kafka-1 kafka-consumer-groups \
    --bootstrap-server kafka-1:29092 \
    --list'
echo ""

# Step 6: Expected Flow
echo -e "${YELLOW}Expected End-to-End Flow:${NC}"
echo ""
echo "  1. POST /api/teachers → TeacherService creates teacher in DB"
echo "  2. TeacherService publishes TeacherCreatedEvent to Kafka"
echo "  3. Kafka stores event in topic 'emis.teacher.created'"
echo "  4. AuthService consumes event from Kafka"
echo "  5. AuthService creates authentication account"
echo "  6. POST /api/auth/login → Returns JWT token ✅"
echo ""

echo -e "${BLUE}========================================${NC}"
echo -e "${GREEN}Ready to test! Good luck! 🚀${NC}"
echo -e "${BLUE}========================================${NC}"
