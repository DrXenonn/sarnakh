namespace OppMonitor.Api.Pipeline.Steps;

public class KeywordMatchStep : IPipelineStep
{
    private readonly ILogger<KeywordMatchStep> _logger;

    public KeywordMatchStep(ILogger<KeywordMatchStep> logger)
    {
        _logger = logger;
    }

    public async Task<TelegramMessageContext?> ExecuteAsync(
            TelegramMessageContext telegramMessageCtx,
            CancellationToken ct)
    {
        var text = telegramMessageCtx.Text;
        var keywords = telegramMessageCtx.Process.Config.Keywords;
        int score = 0;

        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword.Key))
            {
                score += keyword.Value;
            }
        }

        telegramMessageCtx.Score += score;

        if (score <= telegramMessageCtx.Process.Config.ScoreThreshold)
        {
            _logger.LogInformation("Message {MessageId} dropped, score {Score} below threshold", telegramMessageCtx.MessageId, score);
            return null;
        }

        return telegramMessageCtx;
    }
}
