namespace EduCore.API.DTOs.User
{
    public class UpdateUserProfileRequest
    {
        public string? Name { get; set; }
        public IFormFile? Avata { get; set; }
        public string? Password { get; set; }
    }
}
