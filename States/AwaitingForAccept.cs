using Telegram.Bot.Types;
using Telegram.Bot;
using TgShared;
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
            if (message == null || message.Text == null)
                return;
            if (message.Text == "/start")
            {
                //добавить проверку на то, что у пользователя вообще есть доступ (лист с userId). Если он есть и пользователь нажал принять, то даем ему пробный период
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        //показать условия
                        InlineKeyboardButton.WithCallbackData("✅ Принять", "accept"),
                        InlineKeyboardButton.WithCallbackData("❌ Отклонить", "decline")
                    }
                });
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: ShowConditions(), 
                    replyMarkup:inlineKeyboard,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
                );
                session.CurrentState = new GettingAccept(_settings);
            }

        }
        private string ShowConditions()
        {
            return
        @"👋 Добро пожаловать !

Вы используете сервис, предоставляющий инструменты автоматизации и управления через Telegram-бота. Данный бот предоставляет удобный интерфейс для работы с различными функциями (например, загрузка файлов, просмотр данных и др.).

🔒 Условия использования

Перед использованием сервиса внимательно ознакомьтесь с условиями ниже. Нажимая кнопку «✅ Принять», вы подтверждаете своё согласие:

1. Вы обязуетесь использовать сервис исключительно в рамках действующего законодательства Российской Федерации.

2. Все действия, совершаемые с помощью данного бота, выполняются по вашей инициативе и под вашу полную ответственность.

3. Разработчик бота не несёт ответственности за:
   – любые противоправные действия пользователя;
   – возможные блокировки, санкции или ограничения, наложенные сторонними платформами;
   – утрату доступа, данных или иной ущерб, возникший в результате использования сервиса.

4. В случае нарушений законодательства РФ, полную юридическую ответственность несёт исключительно пользователь, принявший данные условия.

5. Для соблюдения прозрачности при принятии условий, в базу данных сохраняется ваш Telegram userId и дата/время согласия.

⚠️ Если вы не согласны с этими условиями, пожалуйста, не используйте сервис.

Нажимая «✅ Принять», вы подтверждаете, что ознакомлены с условиями и добровольно принимаете их.";
        }

    }
}
