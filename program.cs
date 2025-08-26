using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static readonly string BotToken = "7856951736:AAGmgtXtMtLNvJ6HwM_NGrCEdwvXqroMUfY";
    private static readonly string ApiBaseUrl = "https://localhost:7246/api/customer";

    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(BotToken);

        using var cts = new CancellationTokenSource();

        // Настраиваем обработчик обновлений
        botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cts.Token);

        Console.WriteLine("Бот запущен. Нажмите Enter для завершения.");
        Console.ReadLine();

        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обрабатываем только текстовые сообщения
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;

        if (messageText.StartsWith("/get"))
        {
            // Пример: пользователь отправляет "/get data"
            string endpoint = messageText.Substring(5).Trim();

            try
            {
                string response = await GetFromApiAsync(endpoint);
                await botClient.SendTextMessageAsync(chatId, $"Ответ от API:\n{response}", cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(chatId, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Используйте команду /get {endpoint}", cancellationToken: cancellationToken);
        }
    }

    private static async Task<string> GetFromApiAsync(string endpoint)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"{ApiBaseUrl}{endpoint}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Ошибка запроса: {response.StatusCode}");

        return await response.Content.ReadAsStringAsync();
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
