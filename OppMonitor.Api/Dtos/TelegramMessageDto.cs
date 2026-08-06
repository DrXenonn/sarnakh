using NodaTime;

namespace OppMonitor.Api.Dtos;

public record TelegramMessageDto
(
 long MessageId,
 Instant Date,
 long SenderId,
 string? Username,
 string Text,
 long GroupId,
 string GroupName
 )
{ }
