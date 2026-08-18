using Orisia.Server.Common.Responses.Gdpr;

namespace Orisia.Server.Domain.Interfaces;

public interface IGdprService
{
    Task<GdprExportResponse> ExportCurrentUserDataAsync();

    Task<GdprDeleteResponse> DeleteCurrentUserDataAsync();
}
