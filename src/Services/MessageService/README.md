# Message Service - Real-time Chat & Messaging

## 📝 Tổng Quan

Message Service là một microservice hoàn chỉnh cung cấp tính năng chat và nhắn tin real-time cho hệ thống EMIS, cho phép giao tiếp giữa phụ huynh và giáo viên một cách dễ dàng và tiện lợi.

## ✨ Tính Năng Chính

### 1. **Direct Messages (Chat 1-1)**
- Chat trực tiếp giữa phụ huynh và giáo viên
- Tự động phát hiện conversation đã tồn tại
- Hỗ trợ file đính kèm (ảnh, tài liệu, video)

### 2. **Student Group Chat (Nhóm Chat Học Sinh)**
- Tự động tạo khi học sinh được phân lớp
- Bao gồm: phụ huynh học sinh + giáo viên phụ trách lớp
- Tự động thêm giáo viên mới khi được phân công

### 3. **Custom Group Chat (Nhóm Chat Tùy Chỉnh)**
- Người dùng tự tạo nhóm chat
- Quản lý thành viên (thêm/xóa)
- Phân quyền admin/owner/member

### 4. **Real-time Features**
- Gửi/nhận tin nhắn real-time qua SignalR
- Typing indicators (đang gõ...)
- Online/offline status
- Read receipts (đã đọc)
- Message notifications

### 5. **Message Features**
- Reply to message (trả lời tin nhắn)
- Edit message (sửa tin nhắn)
- Delete message (xóa tin nhắn)
- Search messages (tìm kiếm tin nhắn)
- File attachments (đính kèm file)

## 🏗️ Kiến Trúc

### Clean Architecture với 4 Layers:

```
Message.API (Presentation Layer)
    ├── Controllers
    │   └── ConversationsController.cs
    ├── Hubs
    │   └── ChatHub.cs (SignalR)
    └── Program.cs

Message.Application (Application Layer)
    ├── Commands
    │   ├── CreateConversationCommand.cs
    │   └── SendMessageCommand.cs
    ├── Queries
    │   ├── GetConversationsQuery.cs
    │   └── GetMessagesQuery.cs
    ├── DTOs
    │   ├── ConversationDto.cs
    │   └── MessageDto.cs
    └── EventHandlers
        ├── StudentAssignedToClassEventHandler.cs
        └── TeacherAssignedToClassEventHandler.cs

Message.Domain (Domain Layer)
    ├── Entities
    │   ├── Conversation.cs
    │   ├── ChatMessage.cs
    │   └── ConversationMember.cs
    ├── Enums
    │   ├── ConversationType.cs
    │   ├── MessageStatus.cs
    │   ├── MemberRole.cs
    │   └── UserType.cs
    └── Repositories
        ├── IConversationRepository.cs
        └── IMessageRepository.cs

Message.Infrastructure (Infrastructure Layer)
    ├── Persistence
    │   └── MessageDbContext.cs (MongoDB)
    ├── Repositories
    │   ├── ConversationRepository.cs
    │   └── MessageRepository.cs
    └── Configuration
        └── MongoDbSettings.cs
```

## 🗄️ Database Schema (MongoDB)

### Conversations Collection
```json
{
  "_id": "ObjectId",
  "Type": "DirectMessage | StudentGroup | CustomGroup",
  "GroupName": "string (for CustomGroup)",
  "GroupAvatar": "string (URL)",
  "StudentId": "Guid (for StudentGroup)",
  "ClassId": "Guid (for StudentGroup)",
  "Members": [
    {
      "UserId": "string",
      "UserName": "string",
      "Avatar": "string",
      "UserType": "Teacher | Parent | Admin",
      "Role": "Member | Admin | Owner",
      "JoinedAt": "DateTime",
      "LastSeenAt": "DateTime",
      "UnreadCount": "int",
      "IsMuted": "bool",
      "HasLeft": "bool",
      "LeftAt": "DateTime"
    }
  ],
  "LastMessage": {
    "Content": "string",
    "SenderId": "string",
    "SenderName": "string",
    "SentAt": "DateTime",
    "HasAttachment": "bool"
  },
  "TotalMessages": "int",
  "CreatedAt": "DateTime",
  "UpdatedAt": "DateTime",
  "IsDeleted": "bool"
}
```

### Messages Collection
```json
{
  "_id": "ObjectId",
  "ConversationId": "ObjectId",
  "SenderId": "string",
  "SenderName": "string",
  "SenderType": "Teacher | Parent | Admin",
  "Content": "string",
  "Status": "Sent | Delivered | Read | Deleted",
  "Attachments": [
    {
      "FileName": "string",
      "FileUrl": "string",
      "FileType": "string",
      "FileSize": "long",
      "ThumbnailUrl": "string"
    }
  ],
  "ReplyToMessageId": "ObjectId",
  "ReplyToContent": "string",
  "ReadBy": [
    {
      "UserId": "string",
      "ReadAt": "DateTime"
    }
  ],
  "SentAt": "DateTime",
  "EditedAt": "DateTime",
  "IsDeleted": "bool",
  "DeletedAt": "DateTime"
}
```

## 🔌 API Endpoints

### REST API

#### Conversations
```
GET    /api/conversations?userId={userId}&skip=0&limit=50
       Lấy danh sách conversations của user

POST   /api/conversations
       Tạo conversation mới
       Body: CreateConversationCommand

GET    /api/conversations/{conversationId}/messages?skip=0&limit=50
       Lấy messages của conversation

POST   /api/conversations/{conversationId}/messages
       Gửi tin nhắn (REST API)
       Body: SendMessageCommand
```

### SignalR Hub: `/chathub`

#### Client → Server Methods
```javascript
// Join conversation (vào phòng chat)
connection.invoke("JoinConversation", conversationId);

// Leave conversation (rời phòng)
connection.invoke("LeaveConversation", conversationId);

// Gửi tin nhắn
connection.invoke("SendMessage", {
    conversationId: "...",
    senderId: "...",
    senderName: "...",
    content: "...",
    attachments: []
});

// Typing indicator
connection.invoke("Typing", conversationId, userName);
connection.invoke("StopTyping", conversationId, userName);

// Mark as read
connection.invoke("MarkAsRead", conversationId, messageId);
```

#### Server → Client Events
```javascript
// Nhận tin nhắn mới
connection.on("ReceiveMessage", (message) => {
    console.log("New message:", message);
});

// User typing
connection.on("UserTyping", (data) => {
    console.log(`${data.UserName} is typing...`);
});

// Message read
connection.on("MessageRead", (data) => {
    console.log(`Message ${data.MessageId} read by ${data.UserId}`);
});

// User online/offline
connection.on("UserOnline", (userId) => {
    console.log(`User ${userId} is online`);
});

connection.on("UserOffline", (userId) => {
    console.log(`User ${userId} is offline`);
});

// Error
connection.on("Error", (errorMessage) => {
    console.error("Error:", errorMessage);
});
```

## 🚀 Cách Chạy

### 1. Khởi động MongoDB
```bash
docker-compose up -d messagingdb
```

### 2. Chạy Message API
```bash
cd src/Services/MessageService/Message.API
dotnet run
```

API sẽ chạy tại: `http://localhost:5005`  
Swagger UI: `http://localhost:5005/swagger`  
SignalR Hub: `ws://localhost:5005/chathub`

## 🧪 Testing với JavaScript/TypeScript

### SignalR Client Setup
```bash
npm install @microsoft/signalr
```

### Example Code
```typescript
import * as signalR from "@microsoft/signalr";

// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5005/chathub")
    .withAutomaticReconnect()
    .build();

// Lắng nghe sự kiện
connection.on("ReceiveMessage", (message) => {
    console.log("Received:", message);
});

connection.on("UserTyping", (data) => {
    if (data.IsTyping) {
        console.log(`${data.UserName} đang gõ...`);
    }
});

// Kết nối
await connection.start();
console.log("Connected to SignalR Hub");

// Join conversation
await connection.invoke("JoinConversation", "conversationId");

// Gửi tin nhắn
await connection.invoke("SendMessage", {
    conversationId: "...",
    senderId: "user-123",
    senderName: "John Doe",
    content: "Hello, this is a test message!",
    attachments: []
});
```

## 📊 Event-Driven Integration

### Events Published
Không có events được publish (Message Service chủ yếu consume events)

### Events Consumed
1. **StudentAssignedToClassEvent**
   - Tự động tạo nhóm chat cho học sinh
   - Thêm phụ huynh và giáo viên vào nhóm

2. **TeacherAssignedToClassEvent**
   - Thêm giáo viên vào các nhóm chat của học sinh trong lớp

## 🔧 Configuration

### appsettings.json
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "EMIS_MessageDB",
    "ConversationsCollectionName": "conversations",
    "MessagesCollectionName": "messages"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5005"
      }
    }
  }
}
```

## 📈 Performance & Scalability

### MongoDB Indexes
- `Members.UserId` - Query conversations của user
- `StudentId` - Query student groups
- `Type` - Filter theo loại conversation
- `UpdatedAt` - Sort theo thời gian
- `ConversationId` + `SentAt` - Query messages
- Text index on `Content` - Full-text search

### Caching Strategy
- Sử dụng Redis để cache conversations thường xuyên truy cập
- SignalR backplane với Redis cho horizontal scaling

## 🎯 Use Cases

### 1. Phụ huynh chat với giáo viên chủ nhiệm
```
1. Phụ huynh tìm giáo viên chủ nhiệm
2. Click "Nhắn tin"
3. System kiểm tra conversation 1-1 đã tồn tại chưa
4. Nếu chưa, tạo mới; nếu có rồi, mở conversation
5. Phụ huynh gửi tin nhắn
6. Giáo viên nhận thông báo real-time
```

### 2. Nhóm chat học sinh tự động
```
1. Admin/Giáo viên phân học sinh vào lớp
2. StudentService publish StudentAssignedToClassEvent
3. MessageService nhận event
4. Tự động tạo nhóm chat với:
   - Tên: "Nhóm chat của học sinh [Tên HS]"
   - Members: Phụ huynh + Giáo viên phụ trách lớp
5. Tất cả members có thể chat trong nhóm
```

### 3. Typing Indicator
```
1. User A gõ tin nhắn
2. Client gửi "Typing" event
3. Hub broadcast đến các users khác trong conversation
4. Hiển thị "[User A] đang gõ..."
5. Sau 3 giây không gõ, gửi "StopTyping"
```

## 🔒 Security Considerations

- [ ] Implement JWT authentication
- [ ] Authorize users chỉ truy cập conversations mình là member
- [ ] Rate limiting cho API và SignalR
- [ ] Validate file uploads
- [ ] Sanitize message content (XSS prevention)
- [ ] Encrypt sensitive messages (optional)

## 📝 TODO & Future Enhancements

- [ ] Push notifications (Firebase/APNs)
- [ ] Voice messages
- [ ] Video calls (WebRTC)
- [ ] Message reactions (emoji)
- [ ] Pin important messages
- [ ] Broadcast messages (admin)
- [ ] Chat analytics & reports
- [ ] Message translation
- [ ] Archive conversations
- [ ] Export chat history

## 🤝 Dependencies

- MongoDB.Driver 3.5.0
- Microsoft.AspNetCore.SignalR 1.2.0
- MediatR 13.1.0
- FluentValidation 12.0.0
- EMIS.Contracts (Shared)
- EMIS.EventBus.Kafka (Shared)

## 📚 Tài Liệu Tham Khảo

- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr)
- [MongoDB C# Driver](https://mongodb.github.io/mongo-csharp-driver/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Version**: 1.0.0  
**Last Updated**: October 2025  
**Maintainer**: EMIS Development Team
