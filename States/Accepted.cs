using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;

namespace States
{
    public class Accepted : IState
    {
        private readonly AppSettings _settings;

        public Accepted(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message || update.Message?.Text == null)
                return;

            var message = update.Message;

            switch (message.Text)
            {
                case "📥 Загрузить JSON":
                    session.CurrentState = new WaitingForJson(_settings);
                    await bot.SendMessage(message.Chat.Id, "Пожалуйста, отправьте JSON-файл.");
                    break;

                case "📄 Посмотреть каналы":
                    await ForMenu.ShowChannels(update.Message.Chat.Id, session, bot);
                    await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
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
                    await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                    break;
            }
        }
    }
}
