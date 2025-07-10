using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;
using System.Numerics;
using Telegram.Bot.Types.ReplyMarkups;
namespace States
{
    class WaitingForPhone:IState
    {
        private AppSettings _settings;
        public WaitingForPhone(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            var chatId = update.Message.Chat.Id;
            var message = update.Message;
            if (message.Contact != null)
            {
                string phone = message.Contact.PhoneNumber;
                string fullName = $"{message.Contact.FirstName} {message.Contact.LastName}";
                long telegramId = (long)message.Contact.UserId;

                Database.SaveConsent(message.From.Id, phone, DateTime.UtcNow);
                try
                {
                    if (session.TrialStartedAt == null)
                    {
                        session.TrialStartedAt = DateTime.UtcNow;
                        await bot.SendMessage(chatId, "🎉 Пробный период на 14 дней активирован!");
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "✅ Вы уже активировали пробный период.");
                    }

                    session.CurrentState = new Accepted(_settings);
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
                session.CurrentState = new Accepted(_settings);
            }
            else
            {
                Database.SaveConsent(message.From.Id, "", DateTime.UtcNow);

            }

        }
    }
}
