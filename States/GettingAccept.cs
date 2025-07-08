using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;

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
                    // отправляем данные пользователя в db/json (userid, время когда он принял)
                    try
                    {
                        await ForMenu.ShowMenu(update, session, bot);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
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
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                }
            }
        }
        
    }
}
