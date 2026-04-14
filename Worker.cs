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
    private TelegramBotClient? client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var secrets = Secrets.ReadSecretsData() ?? throw new FileLoadException("Secrets are missing");
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

            var httpClient = new HttpClient(handler);

            var client = new TelegramBotClient(secrets.BotToken, httpClient, stoppingToken);
            if (client is not null)
            {
                logger.LogInformation("Bot running at: {Time}", DateTimeOffset.Now);

                await client.SetMyCommands(
                    [("/start", "Getting information about Bot"), ("/link", "Getting link on MTProto proxy")],
                    BotCommandScope.AllPrivateChats(),
                    cancellationToken: stoppingToken);

                client.OnMessage += OnMessage;
                client.OnError += OnError;

                this.client = client;
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
        logger.LogError("Bot error exception: {message}", exception.Message);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        if (string.IsNullOrEmpty(message.Text)) return;

        logger.LogInformation("Recieved message... {message}", message.Text);

        if (string.Equals(message.Text, "/start", StringComparison.OrdinalIgnoreCase) && client is not null)
        {
            await client.SendMessage(message.Chat.Id, "This bot for getting actual link for using MTProto Proxy");
        }

        if (string.Equals(message.Text, "/link", StringComparison.OrdinalIgnoreCase) && client is not null)
        {
            await client.SendMessage(message.Chat.Id, "Your actual link MTProto Proxy");
        }
    }
}