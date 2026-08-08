using System.Text.Json;
using OppMonitor.Api.Dtos;
using Xunit;

namespace OppMonitor.Api.Tests.Dtos;

public class TelegramBatchDtoTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Fact]
    public void Deserializes_RunningBatch_WithNullSenderId()
    {
        var json = """
        {
          "command": "scan_historical",
          "process_id": "4f1996f4-e041-48ec-861f-ba189be1a6f4",
          "messages": [
            { "message_id": 1, "date": "2026-08-08T05:22:42+00:00", "sender_id": null, "username": null, "text": "test", "group_id": -100123, "group_name": "test group" }
          ],
          "status": "running"
        }
        """;

        var batch = JsonSerializer.Deserialize<TelegramBatchDto>(json, Options);

        Assert.NotNull(batch);
        Assert.Equal("running", batch!.Status);
        Assert.Single(batch.Messages!);
        Assert.Null(batch.Messages![0].SenderId);
    }
}
