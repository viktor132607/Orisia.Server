using Orisia.Server.Common.Responses.Admin;

namespace Orisia.Server.Domain.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> GetAsync();
}
