using System.Threading.Tasks;
using ATM.API.Middleware;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using ATM.Domain.Interfaces.Strategies;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;
using ATM.Infrastructure.Services;
using ATM.Infrastructure.Strategies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ATM.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

            builder.Services.AddScoped<IAtmService, AtmService>();

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ICardRepository, CardRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IAtmCassetteRepository, AtmCassetteRepository>();
            builder.Services.AddScoped<IAtmOperationLogRepository, AtmOperationLogRepository>();
            builder.Services.AddScoped<ICashWithdrawalStrategy, GreedyWithdrawalStrategy>();


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                    await context.Database.MigrateAsync();

                    await DbInitializer.SeedDataAsync(context, passwordHasher);
                }catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при ініціалізації БД: {ex.Message}");
                }
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
