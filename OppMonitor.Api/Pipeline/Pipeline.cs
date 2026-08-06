using System.Text.Json;

namespace OppMonitor.Api.Pipeline;

public class Pipeline
{
    public void RunAsync(string jsonPayload, CancellationToken ct)
    {
        // var message = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
        //
        // var cargo = new TelegramMessageContext
        // {
        //     MessageId = jsonPayload.dece
        // }
    }
}
