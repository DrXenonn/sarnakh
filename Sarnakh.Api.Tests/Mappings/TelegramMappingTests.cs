using AutoMapper;

namespace Sarnakh.Api.Tests.Mappings;

public class TelegramMappingTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TelegramMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
