# MESSAGE SERVICE - HƯỚNG DẪN SỬ DỤNG

## 🎯 Giới Thiệu

Message Service đã được xây dựng hoàn chỉnh với đầy đủ tính năng chat và nhắn tin real-time cho hệ thống EMIS.

## ✅ Các Tính Năng Đã Hoàn Thành

### 1. **Domain Layer** ✅
- ✅ Conversation Entity (với ConversationType: DirectMessage, StudentGroup, CustomGroup)
- ✅ ChatMessage Entity (với file attachments, reply, read receipts)
- ✅ ConversationMember (với role management)
- ✅ Enums: ConversationType, MessageStatus, MemberRole, UserType
- ✅ Repository Interfaces

### 2. **Application Layer** ✅
- ✅ CQRS với MediatR
- ✅ Commands:
  - CreateConversationCommand (tạo conversation)
  - SendMessageCommand (gửi tin nhắn)
- ✅ Queries:
  - GetConversationsQuery (lấy danh sách conversations)
  - GetMessagesQuery (lấy messages)
- ✅ Validators với FluentValidation
- ✅ DTOs: ConversationDto, MessageDto
- ✅ Event Handlers:
  - StudentAssignedToClassEventHandler
  - TeacherAssignedToClassEventHandler

### 3. **Infrastructure Layer** ✅
- ✅ MongoDB Configuration
- ✅ MessageDbContext với indexes optimization
- ✅ ConversationRepository implementation
- ✅ MessageRepository implementation

### 4. **API Layer** ✅
- ✅ SignalR ChatHub với real-time features:
  - JoinConversation / LeaveConversation
  - SendMessage
  - Typing indicators
  - MarkAsRead
  - Online/Offline status
- ✅ REST API Controllers
- ✅ Swagger documentation
- ✅ CORS configuration

### 5. **Integration** ✅
- ✅ Events trong EMIS.Contracts:
  - StudentAssignedToClassEvent
  - TeacherAssignedToClassEvent
- ✅ Event handlers tự động tạo student group
- ✅ Kafka EventBus integration ready

## 🚀 Cách Chạy

### Bước 1: Khởi động MongoDB
```bash
cd /Users/trungthao/Projects/emis-app-backend
docker-compose up -d messagingdb
```

### Bước 2: Chạy Message Service
```bash
cd src/Services/MessageService/Message.API
dotnet run
```

### Bước 3: Truy cập Swagger
Mở trình duyệt: `http://localhost:5005/swagger`

### Bước 4: Test SignalR
Sử dụng JavaScript client hoặc file `Message-Testing.http`

## 📝 API Endpoints

### REST API
```
GET    /api/conversations?userId={userId}
POST   /api/conversations
GET    /api/conversations/{id}/messages
POST   /api/conversations/{id}/messages
```

### SignalR Hub
```
ws://localhost:5005/chathub

Methods:
- JoinConversation(conversationId)
- LeaveConversation(conversationId)
- SendMessage(command)
- Typing(conversationId, userName)
- StopTyping(conversationId, userName)
- MarkAsRead(conversationId, messageId)
```

## 🧪 Testing

### 1. Test REST API
Sử dụng file `Message-Testing.http` trong root folder

### 2. Test SignalR
```javascript
// Install: npm install @microsoft/signalr

import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5005/chathub")
    .build();

await connection.start();

connection.on("ReceiveMessage", (message) => {
    console.log("New message:", message);
});

await connection.invoke("JoinConversation", "conversation-id");
await connection.invoke("SendMessage", {
    conversationId: "...",
    senderId: "user-123",
    senderName: "John Doe",
    content: "Hello!"
});
```

## 📊 Workflow Tự Động

### Khi học sinh được phân lớp:
1. StudentService publish `StudentAssignedToClassEvent`
2. MessageService nhận event
3. Tự động tạo nhóm chat bao gồm:
   - Tên nhóm: "Nhóm chat của học sinh [Tên]"
   - Members: Phụ huynh + Giáo viên phụ trách lớp
   - Type: StudentGroup
4. Tất cả members có thể chat trong nhóm

### Khi giáo viên được phân công vào lớp:
1. TeacherService publish `TeacherAssignedToClassEvent`
2. MessageService nhận event
3. Thêm giáo viên vào các nhóm chat của học sinh trong lớp

## 🗂️ File Structure
```
src/Services/MessageService/
├── Message.API/
│   ├── Controllers/ConversationsController.cs
│   ├── Hubs/ChatHub.cs
│   ├── Program.cs
│   └── appsettings.json
├── Message.Application/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   └── EventHandlers/
├── Message.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Repositories/
├── Message.Infrastructure/
│   ├── Persistence/MessageDbContext.cs
│   ├── Repositories/
│   └── Configuration/
└── README.md
```

## 📚 Documentation
Xem chi tiết tại: `src/Services/MessageService/README.md`

## ⚠️ Lưu Ý

### Cần làm thêm:
1. **Authentication & Authorization**
   - Implement JWT authentication
   - Authorize users chỉ truy cập conversations của mình

2. **File Upload**
   - Implement file upload service
   - Store files (Azure Blob, AWS S3, hoặc local)

3. **Push Notifications**
   - Integrate Firebase/APNs
   - Gửi notification khi có tin nhắn mới

4. **Production Deployment**
   - Configure MongoDB cluster
   - Setup Redis for SignalR backplane
   - Load balancing

## 🎉 Kết Luận

MessageService đã được xây dựng hoàn chỉnh với:
- ✅ Clean Architecture
- ✅ CQRS Pattern
- ✅ MongoDB cho chat messages
- ✅ SignalR cho real-time messaging
- ✅ Event-driven integration
- ✅ Full documentation

Bạn có thể sử dụng ngay hoặc mở rộng thêm các tính năng như voice call, video call, etc.

---
**Created by**: GitHub Copilot  
**Date**: October 26, 2025
