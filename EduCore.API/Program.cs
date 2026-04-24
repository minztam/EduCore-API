using CloudinaryDotNet;
using EduCore.API.Data;
using EduCore.API.Hubs;
using EduCore.API.Repositories;
using EduCore.API.Repositories.Implementations;
using EduCore.API.Repositories.Interfaces;
using EduCore.API.Repositories.ResponseMessage;
using EduCore.API.Service;
using EduCore.API.Service.Email;
using EduCore.API.Service.JWT;
using EduCore.API.Service.Vnpay;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace EduCore.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var allowedOrigins = new[] {
                "http://localhost:3000",                             // Cho phép chạy ở Local
                "https://edu-core-frontend-topaz.vercel.app"         // Cho phép trang web đã deploy trên Vercel
            };

            // --- CẤU HÌNH DATABASE ---
            // 1. Dòng cũ SQL Server (Đã comment)
            // builder.Services.AddDbContext<EduCoreDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Dòng mới PostgreSQL (Bật lên để dùng cho Local Postgres hoặc Supabase)
            builder.Services.AddDbContext<EduCoreDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection")!));

            // 3. Fix lỗi DateTime cho PostgreSQL (Bắt buộc phải có từ bản Npgsql 6.0+)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


            // --- CÁC SERVICE KHÁC (Giữ nguyên) ---
            builder.Services.Configure<VnpayOptions>(builder.Configuration.GetSection("Vnpay"));

            builder.Services.AddScoped<JwtService>();
            builder.Services.AddSingleton(sp => builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>());
            builder.Services.AddTransient<EmailService>();
            builder.Services.AddScoped<ResponseMessageResult>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
            builder.Services.AddScoped<ILessonRepository, LessonRepository>();
            builder.Services.AddScoped<IHomeHeroRepository, HomeHeroRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();

            builder.Services.AddControllers();

            // Google Auth
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                options.Scope.Add("email");
                options.Scope.Add("profile");
            });

            // Cloudinary
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
            });

            // JWT Auth
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });

            builder.Services.AddSignalR();

            // CORS - Quan trọng để React có thể gọi API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.WithOrigins(allowedOrigins) // Thêm link deploy của bạn vào đây sau này
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials());
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token: Bearer {your token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // Configure HTTP pipeline
            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduCore API V1");
            //    c.RoutePrefix = "swagger"; // Để truy cập bằng đường dẫn /swagger
            //});

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}