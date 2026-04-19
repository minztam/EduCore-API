namespace EduCore.API.DTOs.HomeHero
{
    public class HomeHeroRequest
    {
        public string Title { get; set; } = null!;
        public string SubTitle { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string PrimaryButtonText { get; set; } = null!;
        public string PrimaryButtonLink { get; set; } = null!;

        public string SecondaryButtonText { get; set; } = null!;
        public string SecondaryButtonLink { get; set; } = null!;

        public IFormFile? BannerImage { get; set; } = null!;
    }
}
