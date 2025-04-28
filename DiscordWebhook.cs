using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VampireWebhook;

public static class DiscordWebhook
{
    private const string DISCORD_HOOK_FILE_PATH = $"BepInEx/config/hook.txt";
    private static string _webhookUrl;
    private static int msgCount = 0;

    private static readonly HttpClient _http = new();


    // <summary>
    // Loads the webhook URL from the file. Returns true if the URL is valid and the webhook is enabled.
    // </summary>
    internal static async Task<bool> LoadHook()
    {
        if (!File.Exists(DISCORD_HOOK_FILE_PATH))
        {
            Plugin.Logger.LogWarning($"Discord webhook file not found at '{DISCORD_HOOK_FILE_PATH}'. Webhook could not be setup.");
            return false;
        }

        var url = File.ReadAllText(DISCORD_HOOK_FILE_PATH).Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            Plugin.Logger.LogWarning($"Discord webhook URL in '{DISCORD_HOOK_FILE_PATH}' is empty. Webhook could not be fully setup.");
            return false;
        }

        bool ok;
        try
        {
            ok = await PingAsync(url);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"Exception while pinging Discord webhook: {ex.Message}. Webhook disabled.");
            _webhookUrl = null;
            return false;
        }

        if (!ok)
        {
            Plugin.Logger.LogWarning($"Discord webhook URL in '{DISCORD_HOOK_FILE_PATH}' is invalid. Webhook disabled.");
            _webhookUrl = null;
            return false;
        }
        else
        {
            _webhookUrl = url;
            return true;
        }
    }

    // <summary>
    // returns true if the hook is enabled in settings and the last ping went through without an issue.
    // </summary>
    public static bool HookEnabled()
    {
        return Settings.UseDiscordWebhook && !string.IsNullOrWhiteSpace(_webhookUrl);
    }

    // <summary>
    // Sends a message to the Discord webhook. If the webhook is not enabled, it does nothing.
    // </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="hookName">The name of the webhook. Defaults to "Vardoran whispers🕯️".</param>
    /// <param name="forceFreshMessage">If true, the message will be sent as a new message evreytime (highly recommended to avoid formating issues). Defaults to true.</param>
    public static async Task<bool> SendDiscordMessageAsync(string msg, string hookName = "Vardoran whispers", bool forceFreshMessage = true)
    {
        if (!HookEnabled())
            return false;

        if (forceFreshMessage)
        {
            hookName += msgCount % 2 == 0 ? "🧛" : "🦇";
        }

        var payload = JsonSerializer.Serialize(new { username = hookName, content = msg });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        try
        {
            var response = await _http.PostAsync(_webhookUrl, content);
            if (!response.IsSuccessStatusCode)
                Plugin.Logger.LogWarning($"Discord returned {response.StatusCode} when sending message.");
            else
                msgCount++;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Failed to send Discord message: {ex.Message}");
        }

        return false;
    }

    private static async Task<bool> PingAsync(string test_url)
    {
        if (string.IsNullOrWhiteSpace(test_url))
        {
            Plugin.Logger.LogWarning("Webhook URL not loaded. Call Load() first.");
            return false;
        }

        try
        {
            // GET the webhook metadata (no message is sent)
            using var resp = await _http.GetAsync(test_url);
            if (resp.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                Plugin.Logger.LogWarning($"Ping failed: Discord returned {resp.StatusCode}.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[Error] Exception pinging webhook: {ex.Message}");
            return false;
        }
    }
}
