using Telegram.Bot.Types;
using Telegram.Bot;

namespace TgShared
{
    public interface IState
    {
        public Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot);
    }
    
}
