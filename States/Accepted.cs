using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;

namespace States
{
    class Accepted:IState
    {
        private AppSettings _settings;
        public Accepted(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            var message = update.Message;
            switch (message.Text)
            {
                case "📥 Загрузить JSON":
                    session.CurrentState = new WaitingForJson(_settings);
                    try
                    {
                        await bot.SendMessage(message.Chat.Id, "Пожалуйста, отправьте JSON-файл.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                    break;
                case "📄 Посмотреть каналы":
                    try
                    {
                        await ForMenu.ShowChannels(update, session, bot);
                        await ForMenu.ShowMenu(update, session, bot);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                    break;
                case "🎬 Загрузить видео":
                    session.CurrentState = new WaitingForVideo(_settings);
                    try
                    {
                        await bot.SendMessage(message.Chat.Id, "Загрузите видео");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                    
                    break;
                case "🗑️ Удалить канал":
                    session.CurrentState = new DeletingAChannel(_settings);
                    break;
                case "❓ Инфо":
                    try
                    {
                        await ForMenu.ShowInfo(update, session, bot);
                        await ForMenu.ShowMenu(update, session, bot);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                    break;
            }
        }
    }
}
