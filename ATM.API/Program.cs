using Microsoft.EntityFrameworkCore;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Repositories;
using ATM.Infrastructure.Services;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;
using System.Threading.Tasks;

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

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
