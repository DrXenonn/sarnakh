using System.Text.Json;
using AutoMapper;
using OppMonitor.Api.Dtos;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Pipeline;

public class PipelineExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IMapper _mapper;
    private readonly IEnumerable<IPipelineStep> _steps;

    public PipelineExecutor(IMapper mapper, IEnumerable<IPipelineStep> steps)
    {
        _mapper = mapper;
        _steps = steps;
    }

    public async Task RunAsync(string jsonPayload, Process process, CancellationToken ct)
    {
        var batch = JsonSerializer.Deserialize<TelegramBatchDto>(jsonPayload, JsonOptions);

        if (batch is null || batch.Messages is null || batch.Messages.Count == 0)
            return;

        foreach (var messageDto in batch.Messages)
        {
            var cargo = _mapper.Map<TelegramMessageContext>(messageDto);
            cargo.Process = process;

            foreach (var step in _steps)
            {
                var processedCargo = await step.ExecuteAsync(cargo, ct);
                if (processedCargo == null) break;
                cargo = processedCargo;
            }
        }
    }
}
