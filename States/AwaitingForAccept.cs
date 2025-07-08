using Telegram.Bot.Types;
using Telegram.Bot;
using TgUnique;
using Telegram.Bot.Types.ReplyMarkups;
namespace States
{
    public class AwaitingForAccept:IState
    {
        private AppSettings _settings;
        public AwaitingForAccept(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            var message = update.Message;
            if (message.Text == "/start")
            {
                //добавить проверку на то, что у пользователя вообще есть доступ (лист с userId). Если он есть и пользователь нажал принять, то даем ему пробный период
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅ Принять", "accept"),
                        InlineKeyboardButton.WithCallbackData("❌ Отклонить", "decline")
                    }
                });
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: ShowConditions(), 
                    replyMarkup:inlineKeyboard
                );
            }

        }
        private string ShowConditions()
        {
            string conditions = "Условия \n Пожалуйста, подтвердите согласие с условиями использования: ";
            return conditions;
        }
    }
}
