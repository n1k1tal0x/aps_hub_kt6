using System.Runtime.CompilerServices;
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
                    if (!await db.Database.CanConnectAsync())
                    {
                        return Results.Problem("No access to the database");
                    }

                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
            );

            app.MapPost(
                "/api/UpdateEmail",
                async (string SearchName, [FromBody] UpdateEmailBody newMailObj, AppDbContext db) =>
                {
                    if (!await db.Database.CanConnectAsync())
                    {
                        return Results.Problem("No access to the database");
                    }

                    var targets = db.Users.Where(x => x.Name == SearchName);
                    await targets.ForEachAsync(
                        async (User user) =>
                        {
                            user.Email = newMailObj.newEmail;
                        }
                    );
                    await db.SaveChangesAsync();
                    return Results.Ok();
                }
            );

            app.MapPost(
                "/api/transacrtions/User",
                async ([FromBody] Models.User newUser, AppDbContext db) =>
                {
                    if (!await db.Database.CanConnectAsync())
                    {
                        return Results.Problem("No access to the database");
                    }

                    await using var transaction = await db.Database.BeginTransactionAsync();

                    User NewUser = new User
                    {
                        Name = newUser.Name,
                        Email = newUser.Email,
                        Age = newUser.Age,
                    };
                    db.Users.Add(NewUser);
                    await db.SaveChangesAsync();
                    if (NewUser.Age >= 18)
                    {
                        await transaction.CommitAsync();
                        return Results.Ok(NewUser);
                    }
                    else
                    {
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
