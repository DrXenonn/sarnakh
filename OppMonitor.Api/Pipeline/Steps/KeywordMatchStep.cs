namespace OppMonitor.Api.Pipeline.Steps;

public class KeywordMatchStep : IPipelineStep
{
    public Task<TelegramMessageContext?> ExecuteAsync(TelegramMessageContext telegramMessageCtx, CancellationToken ct)
    {
        var text = telegramMessageCtx.Text;
        //TODO make a keywrod DTO for now and a Config model later.
    }
}
