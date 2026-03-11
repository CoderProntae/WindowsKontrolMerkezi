using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace WindowsKontrolMerkezi.Services;

public static class SystemNotificationListenerService
{
    private static UserNotificationListener? _listener;

    public static async Task<bool> InitializeAsync()
    {
        try
        {
            // Use ApiInformation to check if UserNotificationListener is present
            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
                return false;

            _listener = UserNotificationListener.Current;
            var accessStatus = await _listener.RequestAccessAsync();

            if (accessStatus != UserNotificationListenerAccessStatus.Allowed)
                return false;

            _listener.NotificationChanged += Listener_NotificationChanged;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        try
        {
            var notificationId = args.UserNotificationId;
            var notification = sender.GetNotification(notificationId);
            if (notification == null) return;

            var appName = notification.AppInfo.DisplayInfo.DisplayName;
            
            // Get visual elements (text)
            var visual = notification.Notification.Visual;
            var bindings = visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            
            string title = "Yeni Bildirim";
            string body = "";

            if (bindings != null)
            {
                var textElements = bindings.GetTextElements();
                title = textElements.FirstOrDefault()?.Text ?? "Bildirim";
                body = string.Join(" ", textElements.Skip(1).Select(t => t.Text));
            }

            // Add to our internal service
            NotificationService.AddNotification(title, body, appName);
        }
        catch (Exception)
        {
            // Silently handle mapping errors
        }
    }
}
