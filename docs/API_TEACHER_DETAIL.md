# Teacher Detail API

## Endpoint

```
GET /api/teachers/{id}
```

## Mô tả

API này lấy **thông tin chi tiết đầy đủ** của một giáo viên theo ID, bao gồm:
- Thông tin cơ bản của giáo viên (tên, email, phone, địa chỉ, v.v.)
- **Danh sách các lớp học** mà giáo viên đang phụ trách
- **Thông tin chi tiết của từng lớp học** (tên lớp, khối, năm học, số học sinh)
- Tổng số lớp được phân công

## Request

### Headers
```
Authorization: Bearer {jwt_token}
Accept: application/json
```

### Path Parameters
- `id` (Guid): ID của giáo viên cần lấy thông tin

### Example
```http
GET /api/teachers/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Accept: application/json
```

## Response

### Success Response (200 OK)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Nguyen Van",
  "lastName": "A",
  "fullName": "Nguyen Van A",
  "dateOfBirth": "1985-05-15T00:00:00",
  "phone": "0901234567",
  "email": "nguyenvana@school.edu.vn",
  "address": "123 Nguyen Hue, District 1, Ho Chi Minh City",
  "avatarUrl": null,
  "status": "Active",
  "createdAt": "2025-10-26T10:30:00",
  "updatedAt": null,
  "classAssignments": [
    {
      "assignmentId": "7b2c1234-5678-90ab-cdef-1234567890ab",
      "teacherId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "classId": "11111111-1111-1111-1111-111111111111",
      "role": "Homeroom Teacher",
      "assignedAt": "2025-10-26T10:30:00",
      "className": "Lớp 10A1",
      "grade": "Khối 10",
      "academicYear": "2024-2025",
      "totalStudents": 45
    },
    {
      "assignmentId": "8c3d2345-6789-01bc-def2-234567890bcd",
      "teacherId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "classId": "22222222-2222-2222-2222-222222222222",
      "role": "Subject Teacher",
      "assignedAt": "2025-10-26T10:30:00",
      "className": "Lớp 10A2",
      "grade": "Khối 10",
      "academicYear": "2024-2025",
      "totalStudents": 42
    }
  ],
  "totalClassesAssigned": 2
}
```

### Error Response (404 Not Found)

```json
{
  "message": "Teacher with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 not found"
}
```

## Response Fields

### Teacher Information
| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | ID của giáo viên |
| `firstName` | string | Tên |
| `lastName` | string | Họ |
| `fullName` | string | Họ và tên đầy đủ |
| `dateOfBirth` | DateTime? | Ngày sinh |
| `phone` | string? | Số điện thoại |
| `email` | string? | Email |
| `address` | string? | Địa chỉ |
| `avatarUrl` | string? | URL ảnh đại diện |
| `status` | string | Trạng thái (Active, Inactive, OnLeave) |
| `createdAt` | DateTime | Ngày tạo |
| `updatedAt` | DateTime? | Ngày cập nhật |
| `totalClassesAssigned` | int | Tổng số lớp được phân công |

### Class Assignment Information
| Field | Type | Description |
|-------|------|-------------|
| `assignmentId` | Guid | ID của phân công |
| `teacherId` | Guid | ID giáo viên |
| `classId` | Guid | ID lớp học |
| `role` | string? | Vai trò (Homeroom Teacher, Subject Teacher, etc.) |
| `assignedAt` | DateTime | Thời điểm phân công |
| `className` | string | Tên lớp học |
| `grade` | string? | Khối học |
| `academicYear` | string? | Năm học |
| `totalStudents` | int? | Tổng số học sinh trong lớp |

## Business Logic

### Class Information Retrieval

**Hiện tại (Mock Data):**
- Thông tin lớp học được mock trong code để demo
- Có 3 lớp mock với data cụ thể:
  - `11111111-1111-1111-1111-111111111111` → Lớp 10A1 (Khối 10, 45 HS)
  - `22222222-2222-2222-2222-222222222222` → Lớp 10A2 (Khối 10, 42 HS)
  - `33333333-3333-3333-3333-333333333333` → Lớp 11B1 (Khối 11, 40 HS)
- Các ClassId khác sẽ trả về data mặc định

**Trong Production (TODO):**
```csharp
// Sẽ gọi ClassService/StudentService để lấy thông tin thực
private async Task<ClassInfoDto> GetClassInfoByIdAsync(Guid classId, CancellationToken cancellationToken)
{
    // Option 1: HTTP Client
    var response = await _httpClient.GetAsync($"http://class-service/api/classes/{classId}");
    var classInfo = await response.Content.ReadFromJsonAsync<ClassInfoDto>();
    
    // Option 2: gRPC
    var classInfo = await _classServiceClient.GetClassByIdAsync(new GetClassRequest { ClassId = classId.ToString() });
    
    // Option 3: Message Queue (eventual consistency)
    // Subscribe to ClassUpdated events and cache locally
    
    return classInfo;
}
```

### Query Flow

```
1. Client gửi GET request với TeacherId
   ↓
2. Controller nhận request, tạo GetTeacherDetailQuery
   ↓
3. MediatR dispatch query tới GetTeacherDetailQueryHandler
   ↓
4. Handler lấy thông tin teacher từ Repository
   ↓
5. Handler lấy danh sách assignments của teacher
   ↓
6. Với mỗi assignment, lấy thông tin lớp học (hiện tại: mock, production: HTTP/gRPC call)
   ↓
7. Map tất cả data sang TeacherDetailDto
   ↓
8. Return response
```

## Usage Examples

### Example 1: Get teacher detail
```bash
curl -X GET "http://localhost:5000/api/teachers/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Accept: application/json"
```

### Example 2: Using with REST Client (VSCode)
```http
GET http://localhost:5000/api/teachers/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer your-jwt-token
Accept: application/json
```

### Example 3: Using with JavaScript/TypeScript
```typescript
const getTeacherDetail = async (teacherId: string, token: string) => {
  const response = await fetch(
    `http://localhost:5000/api/teachers/${teacherId}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/json'
      }
    }
  );
  
  if (!response.ok) {
    throw new Error('Teacher not found');
  }
  
  const teacher = await response.json();
  console.log(`Teacher: ${teacher.fullName}`);
  console.log(`Total classes: ${teacher.totalClassesAssigned}`);
  
  teacher.classAssignments.forEach(assignment => {
    console.log(`- ${assignment.className} (${assignment.role})`);
  });
  
  return teacher;
};
```

## Testing

### Test file
Sử dụng file `Teacher-Detail-Test.http` để test API:

1. Tạo teacher với assignments:
```http
POST http://localhost:5000/api/teachers
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "Teacher",
  "email": "test@school.edu.vn",
  "phone": "0909123456",
  "assignments": [
    {
      "classId": "11111111-1111-1111-1111-111111111111",
      "role": "Homeroom Teacher"
    }
  ]
}
```

2. Copy `id` từ response

3. Get teacher detail:
```http
GET http://localhost:5000/api/teachers/{copied-id}
```

### Verify results
- ✅ Teacher info returned correctly
- ✅ All assignments included
- ✅ Class information populated (className, grade, etc.)
- ✅ Total classes count matches assignment count

## Performance Considerations

### Current Implementation
- **N+1 Query Issue**: Mỗi assignment cần 1 call để lấy class info
- **Latency**: Nếu gọi external service, latency sẽ tăng theo số lượng assignments

### Optimization Strategies

#### 1. Batch API Call
```csharp
// Thay vì gọi từng class một, gọi batch
var classIds = assignments.Select(a => a.ClassId).ToList();
var classInfos = await _classServiceClient.GetClassesByIdsAsync(classIds);
```

#### 2. Caching
```csharp
// Cache class information trong Redis
var cacheKey = $"class:info:{classId}";
var cached = await _cache.GetAsync<ClassInfoDto>(cacheKey);
if (cached != null) return cached;

var classInfo = await GetFromServiceAsync(classId);
await _cache.SetAsync(cacheKey, classInfo, TimeSpan.FromMinutes(30));
```

#### 3. Local Replica (Event-Driven)
```csharp
// Subscribe to ClassCreated/ClassUpdated events
// Lưu local copy trong TeacherService DB
// Read from local DB instead of calling external service
```

## Future Enhancements

- [ ] Implement actual ClassService integration
- [ ] Add caching layer for class information
- [ ] Implement batch API for multiple class lookups
- [ ] Add pagination for teachers with many assignments
- [ ] Add filtering options (by grade, academic year, etc.)
- [ ] Add sorting options
- [ ] Include teacher's schedule/timetable
- [ ] Include student count per class
- [ ] Add GraphQL support for flexible field selection

## Related Endpoints

- `POST /api/teachers` - Create teacher with assignments
- `POST /api/teachers/{id}/assignments` - Assign class to teacher
- `DELETE /api/teachers/assignments/{assignmentId}` - Remove assignment
- `GET /api/teachers/{id}/assignments` - Get assignments only (simpler version)

## Architecture Notes

### Microservices Pattern
TeacherService giữ reference tới Class (chỉ lưu ClassId), không duplicate data của Class. Thông tin chi tiết lớp học được lấy từ ClassService khi cần (Service-to-Service communication).

### Design Decisions
1. **Aggregate Reference**: TeacherClassAssignment chỉ lưu ClassId, không lưu class details
2. **On-demand Loading**: Class info được load khi query (không pre-load)
3. **Mock for Demo**: Sử dụng mock data để demo API structure trước khi integrate với ClassService
4. **Future-proof**: Code structure sẵn sàng cho việc integrate HTTP Client/gRPC

## Contact

For questions or issues, contact the development team.
