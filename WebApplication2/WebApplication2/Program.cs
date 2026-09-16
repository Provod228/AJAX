namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddTransient<ICounter, Counter>();
            builder.Services.AddScoped<ICounter, Counter>();
            builder.Services.AddSingleton<ICounter, Counter>();
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
