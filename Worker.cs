using MTProtoTG.Features;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MTProtoTG;

#pragma warning disable CA1873

public class Worker(ILogger<Worker> logger, IHostApplicationLifetime host) : BackgroundService
{
    private TelegramBotClient? _telegramBotClient;
    private HttpClient? _httpClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var secrets = Secrets.ReadSecrets() ?? throw new ReadSecretsException("Read secrets error");
            logger.LogInformation("Bot token: {token}", secrets.BotToken);

            var proxy = new WebProxy(secrets.SocksUrl)
            {
                Credentials = new NetworkCredential(secrets.SocksUsername, secrets.SocksPassword)
            };

            var handler = new SocketsHttpHandler
            {
                Proxy = proxy,
                UseProxy = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(15)
            };

            _httpClient = new HttpClient(handler);

            var telegramBotClient = new TelegramBotClient(secrets.BotToken, _httpClient, stoppingToken);
            if (telegramBotClient is not null)
            {
                logger.LogInformation("Bot running at: {Time}", DateTimeOffset.Now);

                _telegramBotClient = telegramBotClient;

                await _telegramBotClient.SetMyCommands(
                    [("/start", "Getting information about Bot"), ("/link", "Getting link on MTProto proxy")],
                    BotCommandScope.AllPrivateChats(),
                    cancellationToken: stoppingToken);

                _telegramBotClient.OnMessage += OnMessage;
                _telegramBotClient.OnError += OnError;
            }
            else
            {
                logger.LogCritical("Bot client is not initialazed");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical("Bot not initialization. Application stopped. {message}", ex.Message);
            host.StopApplication();
        }
    }

    private async Task OnError(Exception exception, HandleErrorSource source)
    {
        logger.LogError("Bot exception: {message}", exception.Message);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        if (string.IsNullOrEmpty(message.Text)) return;

        logger.LogInformation("Recieved message... {message}", message.Text);

        if (string.Equals(message.Text, "/start", StringComparison.OrdinalIgnoreCase) && _telegramBotClient is not null)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "This bot for getting actual link for using MTProto Proxy");
        }

        if (string.Equals(message.Text, "/link", StringComparison.OrdinalIgnoreCase) && _telegramBotClient is not null)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "Your current MTProto Proxy link");
        }
    }
}

class ReadSecretsException(string ex) : Exception(ex);