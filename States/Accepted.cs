using Telegram.Bot.Types;
using Telegram.Bot;
using TgUnique;

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
                    await bot.SendMessage(message.Chat.Id, "Пожалуйста, отправьте JSON-файл.");
                    break;
                case "📄 Посмотреть каналы":
                    await ForMenu.ShowChannels(update,session, bot);
                    await ForMenu.ShowMenu(update, session, bot);
                    break;
                case "🎬 Загрузить видео":
                    session.CurrentState = new WaitingForVideo(_settings);
                    await bot.SendMessage(message.Chat.Id, "Загрузите видео");
                    break;
                case "🗑️ Удалить канал":
                    session.CurrentState = new DeletingAChannel(_settings);
                    break;
                case "❓ Инфо":
                    await ForMenu.ShowInfo(update, session, bot);
                    await ForMenu.ShowMenu(update, session, bot);
                    break;
            }
        }
    }
}
