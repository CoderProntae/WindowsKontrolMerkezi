using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text.Json;
using WindowsKontrolMerkezi.Models;

namespace WindowsKontrolMerkezi.Services;

public static class NotificationService
{
    private static readonly string FilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowsKontrolMerkezi", "notifications.json");

    public static ObservableCollection<NotificationModel> Notifications { get; } = new();

    static NotificationService()
    {
        LoadNotifications();
        PurgeOldNotifications();
    }

    private static void LoadNotifications()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var list = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(json);
                if (list != null)
                {
                    foreach (var n in list.Where(x => !x.IsDeleted).OrderByDescending(x => x.Timestamp))
                        Notifications.Add(n);
                }
            }
        }
        catch { }
    }

    private static void SaveNotifications()
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // We save both active and deleted (history) notifications
            var all = Notifications.ToList();
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var existing = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(json) ?? new();
                // Merge active with existing history
                foreach (var n in Notifications)
                {
                    var old = existing.FirstOrDefault(x => x.Id == n.Id);
                    if (old != null) existing.Remove(old);
                    existing.Add(n);
                }
                all = existing;
            }

            var outJson = System.Text.Json.JsonSerializer.Serialize(all, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, outJson);
        }
        catch { }
    }

    private static void PurgeOldNotifications()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var settings = AppSettingsService.Load();
            var json = File.ReadAllText(FilePath);
            var list = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(json) ?? new();
            
            var now = DateTime.Now;
            var toRemove = new List<NotificationModel>();

            foreach (var n in list)
            {
                var diff = (now - n.Timestamp).TotalDays;
                if (n.IsDeleted)
                {
                    if (diff > settings.NotificationHistoryPurgeDays) toRemove.Add(n);
                }
                else
                {
                    if (diff > settings.NotificationPurgeDays) toRemove.Add(n);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (var r in toRemove) list.Remove(r);
                var outJson = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, outJson);
            }
        }
        catch { }
    }

    public static void AddNotification(string title, string message, string source = "Sistem")
    {
        var settings = AppSettingsService.Load();
        
        var notification = new NotificationModel(
            Guid.NewGuid().ToString(),
            title,
            message,
            source,
            DateTime.Now,
            IsDeleted: false,
            IsRead: false,
            ExpiryDate: DateTime.Now.AddDays(settings.NotificationPurgeDays)
        );
        
        App.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, notification);
            // If DND is OFF, show popup or play sound (TBD - for now just add to list as per requirement)
            // User requested: "ses seda olmadan" if DND is on, but imply standard behavior if off.
            // Requirement says: "DND açıksa ses seda olmadan bildirim yerine buraya koysun"
            // We'll manage UI behavior in MainWindow usually, but Service handles the list.
            SaveNotifications();
        });
    }

    public static void RemoveNotification(string id, bool permanent = false)
    {
        var item = Notifications.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Notifications.Remove(item);
                if (!permanent)
                {
                    var historyItem = item with { IsDeleted = true, Timestamp = DateTime.Now }; // Move to history
                    // We don't add to 'Notifications' observable (active list), but we save it to JSON
                    UpdateInJson(historyItem);
                }
                else
                {
                    RemoveFromJson(id);
                }
            });
        }
    }

    private static void UpdateInJson(NotificationModel item)
    {
        try
        {
            var list = new List<NotificationModel>();
            if (File.Exists(FilePath))
                list = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(File.ReadAllText(FilePath)) ?? new();
            
            var old = list.FirstOrDefault(x => x.Id == item.Id);
            if (old != null) list.Remove(old);
            list.Add(item);

            File.WriteAllText(FilePath, System.Text.Json.JsonSerializer.Serialize(list));
        }
        catch { }
    }

    private static void RemoveFromJson(string id)
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var list = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(File.ReadAllText(FilePath)) ?? new();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                list.Remove(item);
                File.WriteAllText(FilePath, System.Text.Json.JsonSerializer.Serialize(list));
            }
        }
        catch { }
    }

    public static void ClearAll()
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                var fullList = new List<NotificationModel>();
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    fullList = JsonSerializer.Deserialize<List<NotificationModel>>(json) ?? new();
                }

                // Mark currently active ones as deleted
                foreach (var active in Notifications)
                {
                    var index = fullList.FindIndex(x => x.Id == active.Id);
                    if (index >= 0)
                    {
                        fullList[index] = fullList[index] with { IsDeleted = true };
                    }
                }

                File.WriteAllText(FilePath, JsonSerializer.Serialize(fullList, new JsonSerializerOptions { WriteIndented = true }));
                Notifications.Clear();
            }
            catch { Notifications.Clear(); }
        });
    }

    public static List<NotificationModel> GetHistory()
    {
        try
        {
            if (!File.Exists(FilePath)) return new();
            var list = System.Text.Json.JsonSerializer.Deserialize<List<NotificationModel>>(File.ReadAllText(FilePath)) ?? new();
            return list.Where(x => x.IsDeleted).OrderByDescending(x => x.Timestamp).ToList();
        }
        catch { return new(); }
    }
}
