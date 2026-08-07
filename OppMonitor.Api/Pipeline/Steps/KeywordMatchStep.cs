namespace OppMonitor.Api.Pipeline.Steps;

public class KeywordMatchStep : IPipelineStep
{
    public async Task<TelegramMessageContext?> ExecuteAsync(TelegramMessageContext telegramMessageCtx, CancellationToken ct)
    {
        var text = telegramMessageCtx.Text;
        var keywords = telegramMessageCtx.Config.Keywords;
        int score = 0;

        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword.Key))
            {
                score += 1 * keyword.Value;
            }
        }

        telegramMessageCtx.Score += score;

        // TODO Read the threshold from config later
        return score < 1 ? null : telegramMessageCtx;
    }
}
