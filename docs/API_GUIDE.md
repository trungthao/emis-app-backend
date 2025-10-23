# Hướng Dẫn Sử Dụng API - EMIS Student Service

## Base URL
```
Development: http://localhost:5001
Production: https://api.emis.edu.vn
```

## Authentication
Hiện tại chưa có authentication. Sẽ implement JWT trong phase tiếp theo.

## Common Headers
```
Content-Type: application/json
Accept: application/json
```

## Response Format

### Success Response
```json
{
  "isSuccess": true,
  "data": { ... },
  "errorMessage": null,
  "errors": null
}
```

### Error Response
```json
{
  "isSuccess": false,
  "data": null,
  "errorMessage": "Error message here",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

## Student APIs

### 1. Tạo Học Sinh Mới

**Endpoint:** `POST /api/students`

**Request Body:**
```json
{
  "studentCode": "HS001",
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "dateOfBirth": "2018-05-15",
  "gender": 1,
  "placeOfBirth": "Hà Nội",
  "address": "123 Đường ABC, Quận 1, TP.HCM",
  "phone": "0901234567",
  "email": "student@example.com",
  "enrollmentDate": "2024-09-01",
  "bloodType": "O",
  "allergies": "Không có",
  "medicalNotes": "Khỏe mạnh"
}
```

**Field Validation:**
- `studentCode`: Required, max 20 characters, unique
- `firstName`: Required, max 50 characters
- `lastName`: Required, max 50 characters
- `dateOfBirth`: Required, must be in the past
- `gender`: Required, values: 1 (Male), 2 (Female), 3 (Other)
- `enrollmentDate`: Required
- `email`: Valid email format (optional)
- `phone`: 10-11 digits (optional)

**Response:** `201 Created`
```json
{
  "isSuccess": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "studentCode": "HS001",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "fullName": "Nguyễn Văn A",
    "dateOfBirth": "2018-05-15",
    "gender": "Male",
    "placeOfBirth": "Hà Nội",
    "address": "123 Đường ABC, Quận 1, TP.HCM",
    "phone": "0901234567",
    "email": "student@example.com",
    "avatarUrl": null,
    "status": "Active",
    "enrollmentDate": "2024-09-01",
    "bloodType": "O",
    "allergies": "Không có",
    "medicalNotes": "Khỏe mạnh",
    "currentClassId": null,
    "currentClassName": null,
    "createdAt": "2024-10-23T10:00:00Z",
    "updatedAt": null
  },
  "errorMessage": null,
  "errors": null
}
```

**Error Response:** `400 Bad Request`
```json
{
  "isSuccess": false,
  "data": null,
  "errorMessage": null,
  "errors": [
    "Mã học sinh không được để trống",
    "Tên không được để trống"
  ]
}
```

### 2. Lấy Thông Tin Học Sinh

**Endpoint:** `GET /api/students/{id}`

**Path Parameters:**
- `id` (UUID): ID của học sinh

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "studentCode": "HS001",
    "firstName": "Văn A",
    "lastName": "Nguyễn",
    "fullName": "Nguyễn Văn A",
    // ... full student data
  },
  "errorMessage": null,
  "errors": null
}
```

**Error Response:** `404 Not Found`
```json
{
  "isSuccess": false,
  "data": null,
  "errorMessage": "Không tìm thấy học sinh với Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "errors": null
}
```

### 3. Lấy Danh Sách Học Sinh (Coming Soon)

**Endpoint:** `GET /api/students`

**Query Parameters:**
- `pageNumber` (int): Số trang (default: 1)
- `pageSize` (int): Số records mỗi trang (default: 10)
- `searchTerm` (string): Từ khóa tìm kiếm
- `classId` (UUID): Lọc theo lớp
- `status` (int): Lọc theo trạng thái

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "items": [ /* array of students */ ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalCount": 50,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### 4. Cập Nhật Học Sinh (Coming Soon)

**Endpoint:** `PUT /api/students/{id}`

**Request Body:** Tương tự Create Student

**Response:** `200 OK`

### 5. Xóa Học Sinh (Coming Soon)

**Endpoint:** `DELETE /api/students/{id}`

**Response:** `200 OK`

Note: Hệ thống sử dụng soft delete, dữ liệu không bị xóa vĩnh viễn.

### 6. Phân Lớp Cho Học Sinh (Coming Soon)

**Endpoint:** `POST /api/students/{id}/assign-class`

**Request Body:**
```json
{
  "classId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:** `200 OK`

## Grade APIs (Coming Soon)

### Tạo Khối Lớp
**Endpoint:** `POST /api/grades`

### Lấy Danh Sách Khối Lớp
**Endpoint:** `GET /api/grades`

## Class APIs (Coming Soon)

### Tạo Lớp Học
**Endpoint:** `POST /api/classes`

### Lấy Danh Sách Lớp Học
**Endpoint:** `GET /api/classes`

### Phân Công Giáo Viên Chủ Nhiệm
**Endpoint:** `POST /api/classes/{id}/assign-teacher`

## Parent APIs (Coming Soon)

### Tạo Phụ Huynh
**Endpoint:** `POST /api/parents`

### Liên Kết Phụ Huynh với Học Sinh
**Endpoint:** `POST /api/parents/{parentId}/link-student`

## School Year APIs (Coming Soon)

### Tạo Năm Học
**Endpoint:** `POST /api/schoolyears`

### Thiết Lập Năm Học Hiện Tại
**Endpoint:** `POST /api/schoolyears/{id}/set-current`

## Health Check

**Endpoint:** `GET /health`

**Response:** `200 OK`
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy"
    }
  ]
}
```

## Error Codes

| Code | Message | Description |
|------|---------|-------------|
| 400 | Bad Request | Invalid input data |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Duplicate resource (e.g., student code exists) |
| 500 | Internal Server Error | Server error |

## Rate Limiting
Coming in future release.

## Examples

### cURL Examples

#### Create Student
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

#### Get Student
```bash
curl http://localhost:5001/api/students/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### JavaScript/Fetch Example

```javascript
// Create Student
const createStudent = async () => {
  const response = await fetch('http://localhost:5001/api/students', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      studentCode: 'HS001',
      firstName: 'Văn A',
      lastName: 'Nguyễn',
      dateOfBirth: '2018-05-15',
      gender: 1,
      enrollmentDate: '2024-09-01'
    })
  });
  
  const result = await response.json();
  
  if (result.isSuccess) {
    console.log('Student created:', result.data);
  } else {
    console.error('Error:', result.errorMessage || result.errors);
  }
};

// Get Student
const getStudent = async (id) => {
  const response = await fetch(`http://localhost:5001/api/students/${id}`);
  const result = await response.json();
  
  if (result.isSuccess) {
    console.log('Student:', result.data);
  } else {
    console.error('Error:', result.errorMessage);
  }
};
```

### C# HttpClient Example

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };

// Create Student
var studentDto = new {
    StudentCode = "HS001",
    FirstName = "Văn A",
    LastName = "Nguyễn",
    DateOfBirth = new DateTime(2018, 5, 15),
    Gender = 1,
    EnrollmentDate = DateTime.Now
};

var response = await client.PostAsJsonAsync("/api/students", studentDto);
var result = await response.Content.ReadFromJsonAsync<Result<StudentDto>>();

if (result.IsSuccess)
{
    Console.WriteLine($"Created student: {result.Data.FullName}");
}
```

## Testing with Swagger UI

1. Navigate to: http://localhost:5001/swagger
2. Expand the endpoint you want to test
3. Click "Try it out"
4. Fill in the request body/parameters
5. Click "Execute"
6. View the response

## Support

For issues or questions:
- Check [Getting Started Guide](./GETTING_STARTED.md)
- View [API Documentation](http://localhost:5001/swagger)
- Create an issue on GitHub
