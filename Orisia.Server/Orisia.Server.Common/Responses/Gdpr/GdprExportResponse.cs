using Orisia.Server.Common.Responses.Users;

namespace Orisia.Server.Common.Responses.Gdpr;

public class GdprExportResponse
{
    public DateTime RequestedAtUtc { get; set; }
    public UserResponse? User { get; set; }
}
