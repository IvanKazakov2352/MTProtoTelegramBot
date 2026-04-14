namespace MTProtoTG.Features;

interface ISecretsData
{
    string BotToken { get; }
    string SocksUsername { get; }
    string SocksPassword { get; }
    string SocksUrl { get; }
}