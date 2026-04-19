using EduCore.API.Repositories;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories
{
    public interface IUserRepository
    {
        Task<ResponseMessageResult> GetPagedUsersAsync(int pageIndex, int pageSize, string? keyword, string? role, bool? isActive);
        Task<ResponseMessageResult> RegisterAsync(string email, string password, string name);
        Task<ResponseMessageResult> LoginAsync(string email, string password);
        Task<ResponseMessageResult> SearchByNameAsync(string keyword);
        Task<ResponseMessageResult> GetUserStatisticsAsync();
        Task<ResponseMessageResult> ToggleUserStatusAsync(Guid id);
        Task<ResponseMessageResult> GoogleLoginAsync(string token);
        Task<ResponseMessageResult> GetUserByIdAsync(Guid id);
        Task<ResponseMessageResult> UpdateUserProfileAsync(Guid id, string? name = null, IFormFile? avata = null, string? password = null);
        Task<ResponseMessageResult> ChangeRoleAsync(Guid id, string newRole);
        Task<ResponseMessageResult> DeleteUserAsync(Guid id);
        Task<ResponseMessageResult> BulkToggleStatusAsync(List<Guid> ids);
    }
}
