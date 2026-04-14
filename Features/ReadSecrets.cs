using Newtonsoft.Json;

namespace MTProtoTG.Features;

class Secrets
{
    public static ISecretsData? ReadSecretsData()
    {
        var filePath = "/etc/secrets/secrets.json";
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File secrets.json not found");

        var text = File.ReadAllText(filePath);

        return JsonConvert.DeserializeObject<ISecretsData>(text);
    }
}