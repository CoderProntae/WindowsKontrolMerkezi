using System;

namespace WindowsKontrolMerkezi.Models;

public record NotificationModel(
    string Id,
    string Title,
    string Message,
    string SourceApp,
    DateTime Timestamp,
    bool IsDeleted = false,
    bool IsRead = false,
    DateTime? ExpiryDate = null
);
