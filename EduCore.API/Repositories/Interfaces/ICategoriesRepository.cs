using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface ICategoriesRepository
    {
        Task<ResponseMessageResult> GetAllAsync();
        Task<ResponseMessageResult> CreateAsync(string name, string slug, string type, Guid? parentId ,int sortOrder);
        Task<ResponseMessageResult> UpdateAsync(Guid id,string name,string slug,string type,Guid? parentId,int sortOrder);
        Task<ResponseMessageResult> DeleteAsync(Guid id);
        Task<ResponseMessageResult> ToggleStatusAsync(Guid id);
    }
}