using Telegram.Bot;
using Telegram.Bot.Types;
using TgShared;

namespace States
{
    class WaitingForTitle:IState
    {
        private AppSettings _settings;
        public WaitingForTitle(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            if (session.channels == null || session.channels.Count == 0)
            {
                await bot.SendMessage(update.Message.Chat.Id, "❗ Сначала добавьте каналы в базу.");
                session.CurrentState = new Accepted(_settings);
                await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                return;
            }
            var message = update.Message;
            if (message == null)
            {
                await bot.SendMessage(update.Message.Chat.Id, "❗ Пожалуйста, введите корректное название.");
                return;
            }

            session.PendingTitle = message.Text;
            session.CurrentState = new WaitingForVideo(_settings);

            await bot.SendMessage(update.Message.Chat.Id, $"✅ Название сохранено: {session.PendingTitle}");
            await bot.SendMessage(update.Message.Chat.Id, "📥 Теперь отправьте видео для загрузки.");
        }

    }
}
