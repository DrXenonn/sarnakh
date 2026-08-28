namespace Sarnakh.Api.Pipeline.Steps;

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
            if (text.Contains(keyword.Text) && keyword.Weight is not null)
            {
                score += keyword.Weight.Value;
            }
        }

        telegramMessageCtx.Score += score;

        if (score <= telegramMessageCtx.Process.Config.ScoreThreshold)
        {
            return null;
        }

        return telegramMessageCtx;
    }
}
