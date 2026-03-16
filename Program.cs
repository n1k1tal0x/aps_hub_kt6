using System.Runtime.CompilerServices;
using asp_hub_kt7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace asp_hub_kt7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt")
                .CreateLogger();

            Log.Information("Application started");

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
                    Log.Information("Request /api/User started");
                    Log.Information("Checking database connection");
                    if (!await db.Database.CanConnectAsync())
                    {
                        Log.Information("Database is unavailable for /api/User");
                        return Results.Problem("No access to the database");
                    }

                    Log.Information("Adding new user");
                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                    Log.Information("User saved successfully");
                    return Results.Ok();
                }
            );

            app.MapPost(
                "/api/UpdateEmail",
                async (string SearchName, [FromBody] UpdateEmailBody newMailObj, AppDbContext db) =>
                {
                    Log.Information("Request /api/UpdateEmail started");
                    Log.Information("Checking database connection");
                    if (!await db.Database.CanConnectAsync())
                    {
                        Log.Information("Database is unavailable for /api/UpdateEmail");
                        return Results.Problem("No access to the database");
                    }

                    Log.Information("Searching users by name");
                    var targets = db.Users.Where(x => x.Name == SearchName);
                    Log.Information("Updating email for found users");
                    await targets.ForEachAsync(
                        async (User user) =>
                        {
                            user.Email = newMailObj.newEmail;
                        }
                    );
                    await db.SaveChangesAsync();
                    Log.Information("Email update finished");
                    return Results.Ok();
                }
            );

            app.MapPost(
                "/api/transacrtions/User",
                async ([FromBody] Models.User newUser, AppDbContext db) =>
                {
                    Log.Information("Request /api/transacrtions/User started");
                    Log.Information("Checking database connection");
                    if (!await db.Database.CanConnectAsync())
                    {
                        Log.Information("Database is unavailable for /api/transacrtions/User");
                        return Results.Problem("No access to the database");
                    }

                    Log.Information("Starting transaction");
                    await using var transaction = await db.Database.BeginTransactionAsync();

                    User NewUser = new User
                    {
                        Name = newUser.Name,
                        Email = newUser.Email,
                        Age = newUser.Age,
                    };
                    Log.Information("Adding user inside transaction");
                    db.Users.Add(NewUser);
                    await db.SaveChangesAsync();
                    if (NewUser.Age >= 18)
                    {
                        Log.Information("Transaction committed");
                        await transaction.CommitAsync();
                        return Results.Ok(NewUser);
                    }
                    else
                    {
                        Log.Information("Transaction rolled back because age is smaller than 18");
                        await transaction.RollbackAsync();
                        return Results.BadRequest("age smaller 18!");
                    }
                }
            );

            app.UseHttpsRedirection();

            app.Run();
        }

        public class UpdateEmailBody
        {
            public string newEmail { get; set; }
        }
    }
}
