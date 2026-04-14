using Newtonsoft.Json;

namespace MTProtoTG.Features;

class Secrets
{
    public static ISecretsData? ReadSecrets()
    {
        var filePath = "/etc/secrets/secrets.json";
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File secrets.json not found");

        return JsonConvert.DeserializeObject<ISecretsData>(File.ReadAllText(filePath));
    }
}