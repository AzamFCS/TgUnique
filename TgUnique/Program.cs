using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgShared;
using States;
using System.Collections.Concurrent;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        foreach (var kv in config.AsEnumerable())
        {
            Console.WriteLine($"{kv.Key} = {kv.Value}");
        }

        var settings = config.GetSection("AppSettings").Get<AppSettings>();

        var botClient = new TelegramBotClient(settings.BotToken);

        var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
        var botService = new BotService(botClient, settings);

        botClient.StartReceiving(botService.HandleUpdateAsync, botService.HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

        var me = await botClient.GetMe();
        Console.WriteLine($"Bot @{me.Username} started");

        Console.WriteLine("Press Ctrl+C to exit");
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Stopping...");
            cts.Cancel();
        };

        await Task.Delay(-1, cts.Token);
    }
}
class BotService
{
    private readonly ITelegramBotClient _bot;
    private readonly AppSettings _settings;
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    public BotService(ITelegramBotClient bot, AppSettings settings)
    {
        _bot = bot;
        _settings = settings;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update == null) return;

            var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id;
            if (chatId == null) return;

            var session = _sessions.GetOrAdd(chatId.Value, _ => new UserSession
            {
                ChatId = chatId.Value,
                CurrentState = new AwaitingForAccept(_settings),
            });

            await session.CurrentState.HandleUpdateAsync(update, session, bot);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in update handling: {ex}");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"API error: {ex.Message}");
        return Task.CompletedTask;
    }
}
