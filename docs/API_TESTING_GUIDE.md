# API Testing Guide

## 📋 Cách sử dụng file Student.http

### Bước 1: Cài đặt Extension
Cài đặt **REST Client** extension trong VS Code:
- Mở VS Code
- Vào Extensions (Ctrl+Shift+X / Cmd+Shift+X)
- Tìm "REST Client" (tác giả: Huachao Mao)
- Click Install

### Bước 2: Mở file Student.http
```bash
code Student.http
```

### Bước 3: Chạy API Tests

#### Cách 1: Click vào link "Send Request"
- Mỗi request có dòng `###` ở trên
- Hover chuột sẽ thấy link "Send Request"
- Click vào để gửi request

#### Cách 2: Dùng phím tắt
- Đặt con trỏ vào request cần test
- Nhấn `Ctrl+Alt+R` (Windows/Linux) hoặc `Cmd+Alt+R` (Mac)

### Bước 4: Xem kết quả
- Response sẽ hiển thị ở panel bên phải
- Bao gồm: Status Code, Headers, Body (JSON)

---

## 🔄 Workflow Test Đề Xuất

### 1️⃣ Test cơ bản - Tạo học sinh đơn giản
```
1. Request #5: Create Student (Without Parents, Without Class)
2. Copy "id" từ response
3. Thay STUDENT_ID_HERE trong Request #8
4. Request #8: Get Student by ID - Verify thông tin
```

### 2️⃣ Test đầy đủ - Tạo học sinh với phụ huynh và lớp
```
1. Request #1: Get All Classes - Lấy danh sách lớp
2. Copy một "id" của class từ response
3. Thay REPLACE_WITH_REAL_CLASS_ID trong Request #7
4. Request #7: Create Student WITH Parents AND Class
5. Copy "id" của student từ response
6. Request #8: Get Student by ID - Verify đầy đủ thông tin
```

### 3️⃣ Test quản lý phụ huynh
```
1. Dùng STUDENT_ID từ bước trên
2. Request #14: Add Parent to Student
3. Copy PARENT_ID từ response
4. Request #15: Update Parent-Student Relationship
5. Request #8: Get Student by ID - Verify quan hệ đã cập nhật
```

### 4️⃣ Test chuyển lớp
```
1. Request #1: Get All Classes - Chọn lớp mới
2. Copy CLASS_ID khác với lớp hiện tại
3. Request #13: Assign Student to Class
4. Request #8: Get Student by ID - Verify lớp mới
```

### 5️⃣ Test cập nhật thông tin
```
1. Request #10: Update Student (Basic Info)
2. Request #8: Get Student by ID - Verify thay đổi
3. Request #11: Update Student AND Change Class
4. Request #8: Get Student by ID - Verify lớp mới
```

---

## 🎯 Test Cases Chi Tiết

### A. Classes API

| # | Endpoint | Mục đích |
|---|----------|----------|
| 1 | GET /api/classes?onlyActive=true | Lấy danh sách lớp đang hoạt động |
| 2 | GET /api/classes?onlyActive=false | Lấy tất cả lớp (bao gồm inactive) |
| 3 | GET /api/classes?gradeId={id} | Lọc lớp theo khối/cấp |
| 4 | GET /api/classes/{id} | Xem chi tiết 1 lớp |

### B. Students API

| # | Endpoint | Mục đích | Ghi chú |
|---|----------|----------|---------|
| 5 | POST /api/students | Tạo học sinh không có phụ huynh, không có lớp | Đơn giản nhất |
| 6 | POST /api/students | Tạo học sinh với 2 phụ huynh (cha + mẹ) | Có validate phone/email |
| 7 | POST /api/students | Tạo học sinh + phụ huynh + phân lớp | Đầy đủ nhất |
| 8 | GET /api/students/{id} | Xem chi tiết học sinh | Bao gồm phụ huynh & lớp |
| 9 | GET /api/students | Danh sách tất cả học sinh | Chưa có pagination |
| 10 | PUT /api/students/{id} | Cập nhật thông tin cơ bản | Không đổi lớp |
| 11 | PUT /api/students/{id} | Cập nhật + đổi lớp | classId khác null |
| 12 | DELETE /api/students/{id} | Xóa học sinh | Soft delete |

### C. Class Assignment

| # | Endpoint | Mục đích | Business Rules |
|---|----------|----------|----------------|
| 13 | POST /api/students/{id}/assign-class | Phân/chuyển lớp | Kiểm tra capacity |

### D. Parent Management

| # | Endpoint | Mục đích | Ghi chú |
|---|----------|----------|---------|
| 14 | POST /api/students/{id}/parents | Thêm phụ huynh mới | Tự động detect trùng phone/email |
| 15 | PUT /api/students/{id}/parents/{parentId} | Cập nhật quan hệ | Chỉ đổi relationship, không đổi info |
| 16 | DELETE /api/students/{id}/parents/{parentId} | Xóa liên kết | Không xóa parent khỏi DB |

---

## 📝 Data Reference

### Gender Values
```
0 = Male (Nam)
1 = Female (Nữ)
```

### RelationshipType Values
```
1 = Father (Cha)
2 = Mother (Mẹ)
3 = Guardian (Người giám hộ)
4 = Grandparent (Ông/Bà)
5 = Other (Khác)
```

### Status Values
```
ClassStatus:
  1 = Active
  2 = Inactive
  3 = Completed

StudentStatus:
  1 = Active
  2 = Inactive
  3 = Graduated
  4 = Transferred
```

---

## 🔍 Validation Rules

### Student
- ✅ `studentCode` là **bắt buộc** và **duy nhất**
- ✅ `firstName`, `lastName` là **bắt buộc**
- ✅ `dateOfBirth` phải **trước ngày hiện tại**
- ✅ `enrollmentDate` là **bắt buộc**
- ✅ `gender` phải là **0 hoặc 1**

### Parent
- ✅ `firstName`, `lastName`, `phone` là **bắt buộc**
- ✅ `phone` phải **đúng format** (10-11 số)
- ✅ `email` phải **đúng format** (nếu có)
- ✅ Phone hoặc Email **không được trùng** với phụ huynh khác

### Class Assignment
- ✅ Lớp phải có **status = Active**
- ✅ Lớp phải **còn chỗ trống** (currentStudentCount < capacity)
- ✅ Học sinh không thể phân vào **lớp đang học**

---

## 🐛 Troubleshooting

### Lỗi 400 Bad Request
- Kiểm tra format JSON
- Kiểm tra các field bắt buộc
- Kiểm tra giá trị enum (gender, relationshipType)

### Lỗi 404 Not Found
- Kiểm tra ID có đúng không
- Kiểm tra resource đã bị xóa chưa (soft delete)

### Lỗi 500 Internal Server Error
- Kiểm tra API log trong terminal
- Kiểm tra database connection
- Kiểm tra foreign key constraints

### "Lớp đã đầy"
- Chọn lớp khác có `availableSeats > 0`
- Hoặc tạo học sinh không chọn lớp (classId = null)

---

## 💡 Tips

1. **Copy ID nhanh**: Double-click vào GUID trong response để select toàn bộ
2. **Save response**: Click "Save Response" để lưu kết quả
3. **Environment variables**: Cập nhật `@studentId`, `@classId` ở đầu file để reuse
4. **Multiple requests**: Có thể chạy nhiều requests liên tiếp
5. **Auto-format JSON**: Install Prettier extension để format JSON body

---

## 📚 Tài liệu API đầy đủ

Truy cập Swagger UI để xem tài liệu API đầy đủ:
```
http://localhost:5001/swagger
```

---

## 🚀 Quick Start

```bash
# 1. Start Docker services
docker compose up -d

# 2. Run API
cd src/Services/StudentService/Student.API
dotnet run

# 3. Open Student.http in VS Code
# 4. Click "Send Request" trên Request #1
```

---

Chúc bạn test thành công! 🎉
