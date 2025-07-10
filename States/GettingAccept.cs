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
                    session.CurrentState = new WaitingForPhone(_settings);
                    await bot.SendMessage(
                        chatId: update.CallbackQuery.From.Id,
                        text: "📱 Для соблюдения прозрачности при принятии условий, в базу данных сохраняется ваш Telegram userId и дата/время согласия,  а также номер телефона, на который привязан ваш аккауант. \n Нажимая кнопку \"Поделиться номером\", вы еще раз подтверждаете, что согласны с указанными выше условиями, а также даете согласие на обработку ваших персональных данных (номер телефона, UserId) для удостоверения вашей личности",
                        replyMarkup: new ReplyKeyboardMarkup(
                            new[]
                            {
                                KeyboardButton.WithRequestContact("📲 Поделиться номером")
                            })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        });
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
