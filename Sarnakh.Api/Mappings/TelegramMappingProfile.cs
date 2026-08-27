using AutoMapper;
using NodaTime;
using Sarnakh.Api.Dtos;
using Sarnakh.Api.Pipeline;

public class TelegramMappingProfile : Profile
{
    public TelegramMappingProfile()
    {
        CreateMap<TelegramMessageDto, TelegramMessageContext>()
       .ForMember(dest => dest.Date,
               opt =>
               opt.MapFrom(src => Instant.FromDateTimeOffset(src.Date)))
       .ForMember(dest => dest.Score,
               opt => opt.Ignore())
       .ForMember(dest => dest.Process,
               opt => opt.Ignore())
       .ForMember(dest => dest.MatchedKeywords,
               opt => opt.Ignore())
       .ForMember(dest => dest.IsLead,
               opt => opt.Ignore());
    }
}
