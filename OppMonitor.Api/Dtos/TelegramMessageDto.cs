using NodaTime;

namespace OppMonitor.Api.Dtos;

public record TelegramMessageDto
(
 long MessageId,
 DateTimeOffset Date,
 long? SenderId,
 string? Username,
 string Text,
 long GroupId,
 string GroupName
 )
{ }
