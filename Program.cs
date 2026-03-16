using asp_hub_kt7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asp_hub_kt7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string connectionString =
                "Host=localhost;Port=5432;Database=testbd;Username=postgres;Password=qwerty123;";
            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.MapPost(
                "/api/User",
                async ([FromBody] Models.User newUser, AppDbContext db) =>
                {
                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
            );

            app.UseHttpsRedirection();

            app.Run();
        }

        record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
        {
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
