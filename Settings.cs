using BepInEx.Configuration;

namespace VampireWebhook;

internal class Settings
{
    internal static bool UseDiscordWebhook { get; private set; } = true;

    internal static void Initialize(ConfigFile config)
    {
        UseDiscordWebhook = config.Bind("General", "UseDiscordWebhook", true, "If the webhook should send messages").Value;
    }
}