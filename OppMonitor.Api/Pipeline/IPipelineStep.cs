namespace OppMonitor.Api.Pipeline;

public interface IPipelineStep
{
    TelegramMessageContext? ExecuteAsync(TelegramMessageContext telegramMessageCtx, CancellationToken ct);
}
