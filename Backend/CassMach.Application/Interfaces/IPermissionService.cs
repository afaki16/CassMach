using CassMach.Application.Common.Results;

namespace CassMach.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<bool>> HasPermissionAsync(int userId, string permission);
        Task<Result<List<string>>> GetUserPermissionsAsync(int userId);
        void ClearUserPermissionCache(int userId);
    }
}
