using AutoMapper;
using NodaTime;
using OppMonitor.Api.Dtos;
using OppMonitor.Api.Pipeline;

public class TelegramMappingProfile : Profile
{
    public TelegramMappingProfile()
    {
        CreateMap<TelegramMessageDto, TelegramMessageContext>()
       .ForMember(dest => dest.Date,
               opt =>
               opt.MapFrom(src => Instant.FromDateTimeOffset(src.Date)));
    }
}
