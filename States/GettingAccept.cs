using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;
using Telegram.Bot.Types.ReplyMarkups;

namespace States
{
    public class GettingAccept : IState
    {
        private AppSettings _settings;
        public GettingAccept(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackData = update.CallbackQuery.Data;
                var chatId = update.CallbackQuery.Message.Chat.Id;
                if (callbackData == "accept")
                {
                    session.CurrentState = new Accepted(_settings);

                    try
                    {
                        await bot.SendMessage(chatId, "Вы приняли условия. Добро пожаловать! Вот ваше меню:");
                        var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup(
                            new[]
                {
                new[] { new KeyboardButton("📥 Загрузить JSON"), new KeyboardButton("📄 Посмотреть каналы") },
                new[] { new KeyboardButton("🎬 Загрузить видео"), new KeyboardButton("🗑️ Удалить канал") },
                new[] { new KeyboardButton("❓ Инфо") }
                })
                        {
                            ResizeKeyboard = true
                        };

                        await bot.SendMessage(chatId, "Выберите действие:", replyMarkup: keyboard);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session?.UserId ?? 0} ошибка: {ex.Message}");
                    }
                }

                else if (callbackData == "decline")
                {
                    try
                    {
                        await bot.SendMessage(chatId, "Вы отказались от использования программы.");
                        session.CurrentState = new AwaitingForAccept(_settings);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session?.UserId ?? 0} ошибка: {ex.Message}");
                    }
                }
            }
        }
        
    }
}
