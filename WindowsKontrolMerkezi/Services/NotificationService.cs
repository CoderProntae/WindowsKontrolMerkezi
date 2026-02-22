using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WindowsKontrolMerkezi.Models;

namespace WindowsKontrolMerkezi.Services;

public static class NotificationService
{
    public static ObservableCollection<NotificationModel> Notifications { get; } = new();

    public static void AddNotification(string title, string message, string source = "Sistem", string? actionUrl = null, string? actionText = null)
    {
        var notification = new NotificationModel(
            Guid.NewGuid().ToString(),
            title,
            message,
            source,
            DateTime.Now,
            actionUrl,
            actionText
        );
        
        // Add to the beginning of the list (most recent first)
        App.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, notification);
        });
    }

    public static void RemoveNotification(string id)
    {
        var item = Notifications.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Notifications.Remove(item);
            });
        }
    }

    public static void ClearAll()
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Clear();
        });
    }
}
