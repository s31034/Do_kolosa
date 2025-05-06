using kolos1.Services;

namespace kolos1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        builder.Services.AddScoped<IDbService, DbService>();
        
        builder.Services.AddControllers();

        builder.Services.AddOpenApi();

        builder.Services.AddTransient<IDbService, DbService>();

        var app = builder.Build();


        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}