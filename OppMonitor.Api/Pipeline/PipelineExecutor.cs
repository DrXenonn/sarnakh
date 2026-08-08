using System.Text.Json;
using AutoMapper;
using OppMonitor.Api.Dtos;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Pipeline;

public class PipelineExecutor
{
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPipelineStep> _steps;

    public PipelineExecutor(IMapper mapper, IEnumerable<IPipelineStep> steps)
    {
        _mapper = mapper;
        _steps = steps;
    }

    public async Task RunAsync(string jsonPayload, Config config, CancellationToken ct)
    {
        var batch = JsonSerializer.Deserialize<TelegramBatchDto>(jsonPayload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                });

        if (batch is null) return;

        var messages = batch.Messages ?? (batch.Message is not null
            ? new List<TelegramMessageDto> { batch.Message }
            : new List<TelegramMessageDto>());

        foreach (var messageDto in messages)
        {
            var cargo = _mapper.Map<TelegramMessageContext>(messageDto);
            cargo.Config = config;

            foreach (var step in _steps)
            {
                var processedCargo = await step.ExecuteAsync(cargo, ct);
                if (processedCargo == null) break; // skip this message, keep going with the rest
                cargo = processedCargo;
            }
        }
    }
}
