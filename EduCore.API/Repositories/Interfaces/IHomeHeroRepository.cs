using EduCore.API.DTOs.HomeHero;
using EduCore.API.Repositories.ResponseMessage;

namespace EduCore.API.Repositories.Interfaces
{
    public interface IHomeHeroRepository
    {
        Task<ResponseMessageResult> GetAsync();
        Task<ResponseMessageResult> SaveAsync(HomeHeroRequest req);
        Task<ResponseMessageResult> ToggleAsync();
    }
}
