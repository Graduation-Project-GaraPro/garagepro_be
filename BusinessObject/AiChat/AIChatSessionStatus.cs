using System;

namespace BusinessObject.AiChat
{
    // Định nghĩa enum, không phải class
    public enum AIChatSessionStatus
    {
        Pending,     // Chưa bắt đầu
        Active,      // Đang chat
        Completed,   // Đã hoàn thành
        Cancelled    // Bị hủy
    }
}
