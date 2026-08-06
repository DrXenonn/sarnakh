namespace OppMonitor.Api.Pipeline;

public interface IPipelineStep
{
    Task<TelegramMessageContext?> ExecuteAsync(TelegramMessageContext telegramMessageCtx, CancellationToken ct);
}
