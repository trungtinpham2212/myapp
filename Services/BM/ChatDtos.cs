using System;
using System.Collections.Generic;

namespace Services.BM;

public class ChatRoomDto
{
    public long ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAvatarUrl { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatMessageDto
{
    public long ChatMessageId { get; set; }
    public long ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class SendMessageRequestDto
{
    public string MessageText { get; set; } = string.Empty;
}
