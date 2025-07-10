using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgShared;
namespace States
{
    public class WaitingForVideo : IState
    {
        public readonly AppSettings _settings;

        public WaitingForVideo(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            try
            {
                var chatId = update.Message.Chat.Id;

                if (session.channels == null || session.channels.Count == 0)
                {
                    await bot.SendMessage(chatId, "❗ Сначала добавьте каналы в базу.");
                    session.CurrentState = new Accepted(_settings);
                    await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                    return;
                }

                var message = update.Message;

                if (message.Type == MessageType.Video)
                {
                    await Unique.ProcessAndUploadAsync(message, session, bot, _settings);
                    session.CurrentState = new Accepted(_settings);
                    await ForMenu.ShowMenuManually(chatId, session, bot);
                    return;
                }
                if (message.Type == MessageType.Document && message.Document != null)
                {
                    var doc = message.Document;
                    if (IsVideoFile(doc.FileName))
                    {
                        await Unique.ProcessAndUploadAsync(message, session, bot, _settings); //вот здесь ошибка вылетает
                        session.CurrentState = new Accepted(_settings);
                        await ForMenu.ShowMenuManually(chatId, session, bot);
                        return;
                    }
                }

                await HandleInvalidUpload(update, session, bot, chatId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
            }
        }
        private bool IsVideoFile(string fileName)
        {
            var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
            return videoExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private async Task HandleInvalidUpload(Update update, UserSession session, ITelegramBotClient bot, long chatId)
        {
            if (session.VideoAttempts == 0)
            {
                await bot.SendMessage(chatId, "❌ Отправьте, пожалуйста, видео в формате .mp4 или другом поддерживаемом.");
                session.VideoAttempts++;
            }
            else
            {
                await bot.SendMessage(chatId, "⚠️ Не удалось определить видео. Вы возвращены в меню.");
                session.VideoAttempts = 0;
                session.CurrentState = new Accepted(_settings);
                await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
            }
        }
    }
}
