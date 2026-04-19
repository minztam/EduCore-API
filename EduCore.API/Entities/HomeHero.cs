using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_HomeHero")]
    public class HomeHero
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string SubTitle { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string PrimaryButtonText { get; set; } = null!;
        public string PrimaryButtonLink { get; set; } = null!;

        public string SecondaryButtonText { get; set; } = null!;
        public string SecondaryButtonLink { get; set; } = null!;

        public string BannerImage { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
