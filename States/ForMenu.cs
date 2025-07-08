using Telegram.Bot.Types;
using Telegram.Bot;
using TgUnique;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;

namespace States
{
    public class ForMenu
    {
        public static async Task ShowMenu(Update update, UserSession session, ITelegramBotClient bot)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "📥 Загрузить JSON", "📄 Посмотреть каналы" },
                new KeyboardButton[] { "🎬 Загрузить видео", "🗑️ Удалить канал" },
                new KeyboardButton[] { "❓ Инфо" }
            })
            {
                ResizeKeyboard = true 
            };

            await bot.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Выберите действие из меню:",
                replyMarkup: keyboard
            );

        }
        public static async Task ShowChannels(Update update, UserSession session, ITelegramBotClient bot)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📺 <b>Список каналов:</b>\n");

            for (int i = 0; i < session.channels.Count; i++)
            {
                var acc = session.channels[i];
                string shortToken = acc.RefreshToken.Length > 10
                    ? acc.RefreshToken.Substring(0, 5) + "..." + acc.RefreshToken[^5..]
                    : acc.RefreshToken;

                sb.AppendLine($"<b>{i + 1}.</b> {acc.ChannelName}");
                sb.AppendLine($"🔑 Token: <code>{shortToken}</code>");
                sb.AppendLine($"✅ Статус: {(acc.isActive ? "Активен" : "Отключен")}\n");
            }

            await bot.SendMessage(
                chatId: update.Message.Chat.Id,
                text: sb.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }
        public static async Task ShowInfo(Update update, UserSession session, ITelegramBotClient bot)
        {
            var chatId = update.Message.Chat.Id;

            string info =
                "📘 <b>Информация о боте</b>\n\n" +
                "Этот бот предназначен для автоматической загрузки видео на YouTube через предоставленные вами аккаунты.\n\n" +
                "⚙️ <b>Как это работает?</b>\n" +
                "1. Вы создаёте проекты в Google Cloud Console и получаете Refresh Token, Client ID и Client Secret для каждого аккаунта.\n" +
                "2. Вы формируете .json файл с каналами по структуре ниже.\n" +
                "3. Отправляете этот JSON в бот.\n" +
                "4. Загружаете видео — бот сам уникализирует и зальёт его на указанные аккаунты.\n\n" +
                "📂 <b>Формат JSON-файла с каналами</b>:\n" +
                "<code>[\n" +
                "  {\n" +
                "    \"ChannelName\": \"MyChannel1\",\n" +
                "    \"RefreshToken\": \"ya29.a0AfH6SM...\",\n" +
                "    \"ClientId\": \"1234567890-abc.apps.googleusercontent.com\",\n" +
                "    \"ClientSecret\": \"XyZ123ABC456\",\n" +
                "    \"isActive\": true\n" +
                "  },\n" +
                "  {\n" +
                "    \"ChannelName\": \"MyChannel2\",\n" +
                "    \"RefreshToken\": \"ya29.a0AfH6SM...\",\n" +
                "    \"ClientId\": \"1234567890-def.apps.googleusercontent.com\",\n" +
                "    \"ClientSecret\": \"LMN456XYZ789\",\n" +
                "    \"isActive\": true\n" +
                "  }\n" +
                "]</code>\n\n" +
                "✅ Все поля обязательны. Убедитесь, что JSON валиден перед загрузкой.";

            await bot.SendMessage(
                chatId: chatId,
                text: info,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }

    }
}
