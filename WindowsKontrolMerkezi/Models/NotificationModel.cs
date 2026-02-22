using System;

namespace WindowsKontrolMerkezi.Models;

public record NotificationModel(
    string Id,
    string Title,
    string Message,
    string SourceApp,
    DateTime Timestamp,
    string? ActionUrl = null,
    string? ActionText = null,
    bool IsDeleted = false,
    bool IsRead = false,
    DateTime? ExpiryDate = null
);
