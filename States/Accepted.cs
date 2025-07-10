using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;
using Telegram.Bot.Types.Enums;
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
                    Console.WriteLine(update.Message.From.Id);
                    session.CurrentState = new WaitingForJson(_settings);
                    await bot.SendMessage(message.Chat.Id, "Пожалуйста, отправьте JSON-файл.");
                    break;

                case "📄 Посмотреть каналы":
                    await ForMenu.ShowChannels(update.Message.Chat.Id, session, bot);
                    await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                    break;

                case "🎬 Загрузить видео":
                    session.CurrentState = new WaitingForTitle(_settings);
                    await bot.SendMessage(message.Chat.Id, "Перед загрузкой видео, отправьте общее название для всех видео");
                    break;

                case "🗑️ Удалить канал":
                    session.CurrentState = new DeletingAChannel(_settings);
                    break;

                case "❓ Инфо":
                    await ForMenu.ShowInfo(update, session, bot);
                    await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                    break;
            }
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text && update.Message.From.Id == _settings.AdminId)
            {
                var msg = update.Message;
                var chatId = msg.Chat.Id;
                var text = msg.Text.Trim();

                if (chatId == _settings.AdminId)
                {
                    if (text.StartsWith("/add "))
                    {
                        if (long.TryParse(text.Substring(5).Trim(), out long newUserId))
                        {
                            bool added = _settings.AddUserId(newUserId);
                            string response = added
                                ? $"✅ Пользователь {newUserId} добавлен в список."
                                : $"ℹ️ Пользователь {newUserId} уже в списке.";
                            await bot.SendMessage(chatId, response);
                            await ForMenu.ShowMenuManually(chatId, session, bot);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "❌ Неверный формат ID.");
                            await ForMenu.ShowMenuManually(chatId, session, bot);
                        }
                        return;
                    }

                    if (text.StartsWith("/toall "))
                    {
                        string messageToSend = text.Substring(7).Trim();
                        int sent = 0, failed = 0;

                        foreach (var idStr in _settings.AllowedUserIDs)
                        {
                            if (long.TryParse(idStr, out long uid))
                            {
                                try
                                {
                                    await bot.SendMessage(uid, messageToSend);
                                    sent++;
                                    
                                }
                                catch
                                {
                                    failed++;
                                }
                            }
                        }

                        await bot.SendMessage(chatId, $"📬 Отправлено: {sent}, ошибок: {failed}");
                        await ForMenu.ShowMenuManually(chatId, session, bot);
                        return;
                    }
                    if (text.StartsWith("/delete "))
                    {
                        if (long.TryParse(text.Substring(8).Trim(), out long removeId))
                        {
                            bool removed = _settings.RemoveUserId(removeId);
                            string response = removed
                                ? $"🗑️ Пользователь {removeId} удалён из списка."
                                : $"⚠️ Пользователь {removeId} не найден в списке.";
                            await bot.SendMessage(chatId, response);
                            await ForMenu.ShowMenuManually(chatId, session, bot);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "❌ Неверный формат ID.");
                        }
                        return;
                    }

                }
            }

        }
    }
}
