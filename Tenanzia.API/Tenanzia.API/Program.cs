
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Services;
using Stripe;
namespace Tenanzia.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:44302", "http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            // Add services to the container.
            builder.Services.AddDbContext<TenanziaContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ITenantService, TenantService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<SubscriptionLimitService>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                {
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidateLifetime = true,
                  ValidateIssuerSigningKey = true,

                  ValidIssuer = "Tenanzia",
                  ValidAudience = "TenanziaUsers",
                  IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("TENANZIA_SUPER_SECRET_KEY_2026_SHOULD_BE_LONG_ENOUGH_123456789")) 
                };
             });
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                  {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                      Reference = new Microsoft.OpenApi.Models.OpenApiReference
                      {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                      }
                    },
                     new string[] {}
                  }
                });
            });




            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();


            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
