// ============================================
// CHAT CLIENT - JAVASCRIPT/TYPESCRIPT EXAMPLE
// ============================================

import * as signalR from "@microsoft/signalr";

class ChatClient {
    private connection: signalR.HubConnection;
    private currentUserId: string;
    private currentUserName: string;

    constructor(userId: string, userName: string) {
        this.currentUserId = userId;
        this.currentUserName = userName;

        // Khởi tạo SignalR connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5005/chathub")
            .withAutomaticReconnect() // Auto reconnect
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.setupEventHandlers();
    }

    // ==========================================
    // EVENT HANDLERS (Lắng nghe từ server)
    // ==========================================
    private setupEventHandlers() {
        // Nhận tin nhắn mới
        this.connection.on("ReceiveMessage", (message) => {
            console.log("📨 New message:", message);
            this.displayMessage(message);
        });

        // User đang gõ
        this.connection.on("UserTyping", (data) => {
            if (data.IsTyping) {
                console.log(`⌨️  ${data.UserName} đang gõ...`);
                this.showTypingIndicator(data.UserName);
            } else {
                this.hideTypingIndicator(data.UserName);
            }
        });

        // Tin nhắn đã được đọc
        this.connection.on("MessageRead", (data) => {
            console.log(`✅ Message ${data.MessageId} read by ${data.UserId}`);
            this.markMessageAsRead(data.MessageId, data.UserId);
        });

        // User online/offline
        this.connection.on("UserOnline", (userId) => {
            console.log(`🟢 User ${userId} is online`);
            this.updateUserStatus(userId, "online");
        });

        this.connection.on("UserOffline", (userId) => {
            console.log(`⚫ User ${userId} is offline`);
            this.updateUserStatus(userId, "offline");
        });

        // Error handling
        this.connection.on("Error", (errorMessage) => {
            console.error("❌ Error:", errorMessage);
            this.showError(errorMessage);
        });
    }

    // ==========================================
    // KẾT NỐI & DISCONNECT
    // ==========================================
    async connect() {
        try {
            await this.connection.start();
            console.log("✅ Connected to SignalR Hub");
        } catch (error) {
            console.error("❌ Connection failed:", error);
            // Retry sau 5 giây
            setTimeout(() => this.connect(), 5000);
        }
    }

    async disconnect() {
        await this.connection.stop();
        console.log("👋 Disconnected from SignalR Hub");
    }

    // ==========================================
    // CHAT ACTIONS
    // ==========================================

    // ✅ GỬI TIN NHẮN (Cách được khuyến nghị)
    async sendMessage(conversationId: string, content: string, attachments = []) {
        try {
            await this.connection.invoke("SendMessage", {
                conversationId: conversationId,
                senderId: this.currentUserId,
                senderName: this.currentUserName,
                content: content,
                attachments: attachments
            });
            console.log("✅ Message sent");
        } catch (error) {
            console.error("❌ Failed to send message:", error);
            
            // Fallback: Gửi qua REST API nếu SignalR fail
            await this.sendMessageViaRestAPI(conversationId, content, attachments);
        }
    }

    // Fallback: Gửi qua REST API
    private async sendMessageViaRestAPI(conversationId: string, content: string, attachments = []) {
        const response = await fetch(`http://localhost:5005/api/conversations/${conversationId}/messages`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                conversationId: conversationId,
                senderId: this.currentUserId,
                senderName: this.currentUserName,
                content: content,
                attachments: attachments
            })
        });

        if (response.ok) {
            console.log("✅ Message sent via REST API (fallback)");
        } else {
            throw new Error("Failed to send message");
        }
    }

    // Join conversation (vào phòng chat)
    async joinConversation(conversationId: string) {
        await this.connection.invoke("JoinConversation", conversationId);
        console.log(`✅ Joined conversation ${conversationId}`);
    }

    // Leave conversation
    async leaveConversation(conversationId: string) {
        await this.connection.invoke("LeaveConversation", conversationId);
        console.log(`👋 Left conversation ${conversationId}`);
    }

    // Typing indicator
    async startTyping(conversationId: string) {
        await this.connection.invoke("Typing", conversationId, this.currentUserName);
    }

    async stopTyping(conversationId: string) {
        await this.connection.invoke("StopTyping", conversationId, this.currentUserName);
    }

    // Mark as read
    async markAsRead(conversationId: string, messageId: string) {
        await this.connection.invoke("MarkAsRead", conversationId, messageId);
    }

    // ==========================================
    // UI HELPERS (implement theo UI framework của bạn)
    // ==========================================
    private displayMessage(message: any) {
        // TODO: Implement UI logic
        console.log("Display message:", message);
    }

    private showTypingIndicator(userName: string) {
        // TODO: Show "User is typing..."
    }

    private hideTypingIndicator(userName: string) {
        // TODO: Hide typing indicator
    }

    private markMessageAsRead(messageId: string, userId: string) {
        // TODO: Update UI to show message read
    }

    private updateUserStatus(userId: string, status: "online" | "offline") {
        // TODO: Update user status in UI
    }

    private showError(message: string) {
        // TODO: Show error notification
    }
}

// ==========================================
// USAGE EXAMPLE
// ==========================================

async function main() {
    // Khởi tạo chat client
    const chatClient = new ChatClient("parent-123", "Nguyễn Văn A");

    // Kết nối
    await chatClient.connect();

    // Join conversation
    const conversationId = "conv-abc-123";
    await chatClient.joinConversation(conversationId);

    // Gửi tin nhắn
    await chatClient.sendMessage(
        conversationId,
        "Xin chào cô giáo, con em hôm nay có nghỉ học được không ạ?"
    );

    // Typing indicator
    const messageInput = document.getElementById("messageInput");
    let typingTimeout: any;

    messageInput?.addEventListener("input", async () => {
        await chatClient.startTyping(conversationId);

        // Stop typing sau 3 giây không gõ
        clearTimeout(typingTimeout);
        typingTimeout = setTimeout(async () => {
            await chatClient.stopTyping(conversationId);
        }, 3000);
    });

    // Gửi tin nhắn khi nhấn Enter
    messageInput?.addEventListener("keypress", async (e) => {
        if (e.key === "Enter") {
            const content = (e.target as HTMLInputElement).value;
            await chatClient.sendMessage(conversationId, content);
            (e.target as HTMLInputElement).value = "";
            await chatClient.stopTyping(conversationId);
        }
    });
}

// Run
main();

export { ChatClient };
