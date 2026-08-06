using AutoMapper;
using OppMonitor.Api.Dtos;
using OppMonitor.Api.Pipeline;

public class TelegramMappingProfile : Profile
{
    public TelegramMappingProfile()
    {
        CreateMap<TelegramMessageDto, TelegramMessageContext>();
    }
}
