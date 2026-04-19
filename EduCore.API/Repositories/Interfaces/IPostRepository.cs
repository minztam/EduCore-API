using EduCore.API.DTOs.Post;
using EduCore.API.Entities;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<ResponseMessageResult> GetAllAsync(PostQuery query);
        Task<ResponseMessageResult> GetByIdAsync(Guid id);
        Task<ResponseMessageResult> GetBySlugAsync(string slug);
        Task<ResponseMessageResult> CreateAsync(PostRequest model);
        Task<ResponseMessageResult> UpdateAsync(Guid id, PostRequest model);
        Task<ResponseMessageResult> DeleteAsync(Guid id);
        Task<ResponseMessageResult> ChangePublishStatusAsync(Guid id);
        Task<ResponseMessageResult> IncreaseViewAsync(string slug);
        Task<ResponseMessageResult> ChangeFeaturedStatusAsync(Guid id);
        Task<ResponseMessageResult> GetFeaturedPostsAsync(int count);
    }
}
