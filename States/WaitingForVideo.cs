using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgUnique;

namespace States
{
    class WaitingForVideo:IState
    {
        private AppSettings _settings;
        public WaitingForVideo(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            int cnt = 0;

            if (session.channels.Count == 0)
            {
                await bot.SendMessage(update.Message.Chat.Id,"Сначала добавьте каналы в базу");
                await ForMenu.ShowMenu(update, session, bot);
                session.CurrentState = new Accepted(_settings);
            }
            if (update.Message.Type == MessageType.Video)
            {
                var video = update.Message.Video;
                //логика залива
            }
            else
            {
                if (cnt == 0)
                {
                    await bot.SendMessage(update.Message.Chat.Id, "Отправьте, пожалуйста, Видео");
                    cnt = 1;
                }
                else
                {
                    await bot.SendMessage(update.Message.Chat.Id, "Не видео. Вы возвращены в меню");
                    session.CurrentState = new Accepted(_settings);
                    await ForMenu.ShowMenu(update, session, bot);
                    cnt = 0;
                }
            }
        }
    }
}
