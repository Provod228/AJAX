using System.Globalization;
using WebApplication4.Infrastructure;
using WebApplication4.Services;   // ← обязательно, иначе не увидит типы

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 👇 ВОТ ЭТОЙ СТРОКИ НЕ ХВАТАЛО
            builder.Services.AddScoped<IProductRepository, JsonProductRepository>();

            // REST из конспекта: идемпотентность POST/PATCH, ProblemDetails, глобальные ошибки
            builder.Services.AddSingleton<IdempotencyService>();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // en-US по умолчанию (конспект): decimal в JSON/формах парсится с точкой
            builder.Services.Configure<RequestLocalizationOptions>(o =>
            {
                o.SetDefaultCulture("en-US");
                o.SupportedCultures = [new CultureInfo("en-US")];
                o.SupportedUICultures = [new CultureInfo("en-US")];
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseExceptionHandler(); // ProblemDetails на необработанные исключения (401/403/404/500)
            app.UseRequestLocalization();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}