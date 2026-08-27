using Microsoft.Extensions.Logging.Abstractions;
using Sarnakh.Api.Models;
using Sarnakh.Api.Pipeline;
using Sarnakh.Api.Pipeline.Steps;

namespace Sarnakh.Api.Tests.PipeLineSteps;

public class KeywordMatchStepTests
{
    private readonly KeywordMatchStep _keywordStep;

    public KeywordMatchStepTests()
    {
        _keywordStep = new KeywordMatchStep(
            new NullLogger<KeywordMatchStep>()
        );
    }

    [Theory]
    [InlineData("test", false, 0)]
    [InlineData("یک متن فارسی خوب", true, 6)]
    [InlineData("A long text which has no purpose", false, 0)]
    [InlineData("Another good test which keywords will appear in", true, 10)]
    public async Task Checks_Keywords_And_Scores_Correctly(
        string text,
        bool expectMatch,
        int expectScore)
    {
        var cargo = new TelegramMessageContext
        {
            Text = text,
            Process = new Process
            {
                Config = new Config
                {
                    Keywords =
                    [
                        new Keyword
                        {
                            Text = "NotTeeest",
                            Weight = 10
                        },
                        new Keyword
                        {
                            Text = "فارسی",
                            Weight = 6
                        },
                        new Keyword
                        {
                            Text = "good",
                            Weight = 5
                        },
                        new Keyword
                        {
                            Text = "will appear",
                            Weight = 5
                        }
                    ]
                }
            }
        };

        var processedCargo = await _keywordStep.ExecuteAsync(
            cargo,
            CancellationToken.None
        );

        if (expectMatch)
        {
            Assert.NotNull(processedCargo);
            Assert.Equal(expectScore, processedCargo.Score);
        }
        else
        {
            Assert.Null(processedCargo);
        }
    }
}
