using System.Text.Json;

public class AppSettings
{
    public string BotToken { get; set; }
    public string FfmpegPath { get; set; }
    public string UploadsDirectory { get; set; }
    public string[] AllowedUserIDs { get; set; }
    public string DatabasePath { get; set; }
    public string FfprobePath { get; set; }
    public long AdminId { get; set; }

    private static readonly string ConfigPath = "appsettings.json";

    public static AppSettings Load()
    {
        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppSettings>(json);
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    public bool AddUserId(long userId)
    {
        if (AllowedUserIDs.Contains(userId.ToString()))
            return false;

        var updatedList = AllowedUserIDs.ToList();
        updatedList.Add(userId.ToString());
        AllowedUserIDs = updatedList.ToArray();
        Save();
        return true;
    }
    public bool RemoveUserId(long userId)
    {
        var idStr = userId.ToString();
        if (!AllowedUserIDs.Contains(idStr))
            return false;

        var updatedList = AllowedUserIDs.ToList();
        updatedList.Remove(idStr);
        AllowedUserIDs = updatedList.ToArray();
        Save();
        return true;
    }
}
