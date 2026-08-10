using Microsoft.Extensions.Logging.Abstractions;
using OppMonitor.Api.Models;
using OppMonitor.Api.Pipeline;
using OppMonitor.Api.Pipeline.Steps;

namespace OppMonitor.Api.Tests.PipeLineSteps;

public class KeywordMatchStepTests
{
    private KeywordMatchStep _keywordStep;

    public KeywordMatchStepTests()
    {
        _keywordStep = new KeywordMatchStep(new NullLogger<KeywordMatchStep>());
    }

    [Theory]
    [InlineData("test", false, 0)]
    [InlineData("یک متن فارسی خوب", true, 6)]
    [InlineData("A long text which has no purpose", false, 0)]
    [InlineData("Another good test which keywords will appear in", true, 10)]
    public async Task Checks_Keywords_And_Scores_Correctly(string text, bool expectMatch, int expectScore)
    {
        var cargo = new TelegramMessageContext
        {
            Text = text,
            Process = new Process
            {
                Config = new Config()
                {
                    Keywords = new Dictionary<string, int>
                    {
                        {"NotTeeest", 10},
                        {"فارسی", 6},
                        {"good", 5},
                        {"will appear", 5}
                    }
                }
            }
        };

        var processedCargo = await _keywordStep.ExecuteAsync(cargo, CancellationToken.None);

        if (expectMatch)
        {
            Assert.NotNull(processedCargo);
            Assert.Equal(processedCargo?.Score, expectScore);
        }
        else
        {
            Assert.Null(processedCargo);
        }
    }
}
