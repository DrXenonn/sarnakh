using System.Text.Json;
using AutoMapper;
using OppMonitor.Api.Dtos;

namespace OppMonitor.Api.Pipeline;

public class Pipeline
{
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPipelineStep> _steps;

    public Pipeline(IMapper mapper, IEnumerable<IPipelineStep> steps)
    {
        _mapper = mapper;
        _steps = steps;
    }

    public async Task RunAsync(string jsonPayload, CancellationToken ct)
    {
        var message = JsonSerializer.Deserialize<TelegramMessageDto>(jsonPayload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

        var cargo = _mapper.Map<TelegramMessageContext>(message);

        foreach (var step in _steps)
        {
            var processedCargo = await step.ExecuteAsync(cargo, ct);
            if (processedCargo == null) return;
        }
    }
}
