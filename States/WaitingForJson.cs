using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using TgShared;

namespace States
{
    public class WaitingForJson : IState
    {
        private AppSettings _settings;
        public WaitingForJson(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task HandleUpdateAsync(Update update, UserSession session, ITelegramBotClient bot)
        {
            try
            {
                if (update?.Message == null)
                    return;

                var chatId = update.Message.Chat.Id;

                if (update.Message.Type == MessageType.Document && update.Message.Document != null)
                {
                    var document = update.Message.Document;

                    if (document.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var file = await bot.GetFile(document.FileId);
                            using var stream = new MemoryStream();
                            await bot.DownloadFile(file.FilePath, stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using var reader = new StreamReader(stream);
                            var jsonContent = await reader.ReadToEndAsync();

                            var accs = JsonConvert.DeserializeObject<List<YouTubeAcc>>(jsonContent);

                            if (accs != null)
                            {
                                await bot.SendMessage(chatId, $"✅ Принято {accs.Count} аккаунтов");
                                session.channels = accs;
                                session.JsonAttempts = 0;
                                await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
                                session.CurrentState = new Accepted(_settings);
                                return;
                            }

                            await bot.SendMessage(chatId, "❌ Не удалось распознать JSON-файл.");
                        }
                        catch (Exception ex)
                        {
                            await bot.SendMessage(chatId, "❌ Ошибка при обработке JSON. Попробуйте заново.");
                            Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
                        }
                    }
                    else
                    {
                        await HandleInvalidUpload(update, session, bot, chatId);
                    }
                }
                else
                {
                    await HandleInvalidUpload(update, session, bot, chatId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{session.UserId} ошибка: {ex.Message}");
            }
        }

        private async Task HandleInvalidUpload(Update update, UserSession session, ITelegramBotClient bot, long chatId)
        {
            if (session.JsonAttempts == 0)
            {
                await bot.SendMessage(chatId, "❌ Отправьте, пожалуйста, JSON файл как документ.");
                session.JsonAttempts++;
            }
            else
            {
                await bot.SendMessage(chatId, "📥 Не является JSON файлом. Возвращаем вас в меню.");
                session.JsonAttempts = 0;
                session.CurrentState = new Accepted(_settings);
                await ForMenu.ShowMenuManually(update.Message.Chat.Id, session, bot);
            }
        }
    }

}
