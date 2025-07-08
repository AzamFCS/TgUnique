using Telegram.Bot;
using Telegram.Bot.Types;
using TgShared;

namespace States
{
    class DeletingAChannel:IState
    {
        private AppSettings _settings;
        public DeletingAChannel() { }
        public DeletingAChannel(AppSettings settings)
        {
            _settings = settings;
        }
        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            await ForMenu.ShowMenu(update, session, bot);
            try
            {
                bot.SendMessage(update.Message.Chat.Id, "Отправьте номер канала, который вы хотите удалить");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
            }
            if (int.TryParse(update.Message.Text,out int res))
            {
                if (res > 0 && res <= session.channels.Count)
                {
                    try
                    {
                        var removed = session.channels[res - 1];
                        session.channels.RemoveAt(res - 1);
                        bot.SendMessage(update.Message.Chat.Id, $"✅ Удалили аккуант {removed.ChannelName}");
                        ForMenu.ShowMenu(update, session, bot);
                        session.CurrentState = new Accepted(_settings);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                    }
                }
                else
                {
                    await HandleInvalidUpload(update, session, bot, update.Message.Chat.Id);    
                }
            }
            else
            {
                await HandleInvalidUpload(update, session, bot, update.Message.Chat.Id);
            }
        }
        private async Task HandleInvalidUpload(Update update, UserSession session, ITelegramBotClient bot, long chatId)
        {
            try
            {
                if (session.JsonAttempts == 0)
                {
                    await bot.SendMessage(chatId, "❌ Введите корректное число.");
                    session.JsonAttempts++;
                }
                else
                {
                    await bot.SendMessage(chatId, "📥 Вы ввели некорректные данные. Возвращаем вас в меню.");
                    session.JsonAttempts = 0;
                    session.CurrentState = new Accepted(_settings);
                    await ForMenu.ShowMenu(update, session, bot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
            }
        }
    }
}
