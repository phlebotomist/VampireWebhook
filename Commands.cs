using VampireCommandFramework;

namespace VampireWebhook;

public class Commands
{
    [Command("reloadHook_", shortHand: "rlh_", description: "Attempts to reload the Discord webhook.", adminOnly: true)]
    public async void ReloadDiscordHook(ChatCommandContext ctx)
    {
        bool issetup = await DiscordWebhook.LoadHook();
        ctx.Reply(issetup ? "Discord Webhook loaded correctly!" : "Something went wrong loading the Discord Webhook. Check the logs or reset the file.");
    }

    [Command("testhook_", shortHand: "th_", description: "Sends a test message to the Discord webhook.", adminOnly: true)]
    public void TestDiscordHook(ChatCommandContext ctx)
    {
        _ = DiscordWebhook.SendDiscordMessageAsync("The bats whisper in the night, and the wolves howl at the moon. The shadows dance, and the darkness calls. The night is alive with secrets, and the stars are our only witnesses. We are the children of the night, and we embrace the darkness.");
        ctx.Reply("Test message sent to Discord webhook. Check the channel for a message about bat whispering in the night to confirm setup.");
    }
}