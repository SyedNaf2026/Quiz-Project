using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using QuizzApp.Context;
using QuizzApp.Hubs;
using QuizzApp.Interfaces;
using QuizzApp.Middleware;
using QuizzApp.Repository;
using QuizzApp.Services;
using System.Text;

namespace QuizzApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================
            // 1. DATABASE CONFIGURATION (EF Core + SQL Server)
            // ============================================================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ============================================================
            // 2. GENERIC REPOSITORY
            // ============================================================
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // ============================================================
            // 3. SERVICES
            // ============================================================
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
            builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // SignalR for real-time notifications
            builder.Services.AddSignalR();
            // ============================================================
            // 4. JWT SETTINGS (Prepared for later use)
            // ============================================================
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new Exception("JWT SecretKey missing");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            // ============================================================
            // 5. ADD CONTROLLERS
            // ============================================================
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Return camelCase JSON so Angular models match (e.g. options, optionText)
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // ============================================================
            // 6. CORS CONFIGURATION (Allow Angular Frontend)
            // ============================================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials(); // Required for SignalR
                    });
            });

            // ============================================================
            // 7. SWAGGER CONFIGURATION
            // ============================================================
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "QuizApp API",
                    Version = "v1",
                    Description = "Quiz Application API with Swagger Documentation"
                });

                // JWT Support in Swagger (for later use)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Token: Bearer {your token}"
                });
            });

            // ============================================================
            // BUILD APPLICATION
            // ============================================================
            var app = builder.Build();

            // ============================================================
            // 8. MIDDLEWARE PIPELINE
            // ============================================================

            // Global exception handler — must be first
            app.UseMiddleware<ExceptionMiddleware>();

            // Enable Swagger
            app.UseSwagger();
            //app.UseSwaggerUI(options =>
            //{
            //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizApp API v1");
            //    options.RoutePrefix = "swagger";
            //});

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // Enable CORS for Angular
            app.UseCors("AllowAngular");

            // Enable Authentication later if JWT added
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

            // Map SignalR hub
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.Run();
        }
    }
}