using NodaTime;
using OppMonitor.Api.Data;

namespace OppMonitor.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public Instant Expires { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
