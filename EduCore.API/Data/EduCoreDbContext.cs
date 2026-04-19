using EduCore.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Data
{
    public class EduCoreDbContext : DbContext
    {
        public EduCoreDbContext(DbContextOptions<EduCoreDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<HomeHero> HomeHeroes { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
