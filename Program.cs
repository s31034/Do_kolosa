using System.Data.SqlClient;

namespace TravelAgencyAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Attempting to connect with: {connectionString.Replace("Password=.*;", "Password=*****;")}");

        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                 connection.OpenAsync();
                Console.WriteLine(" Connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Connection failed: {ex.ToString()}");
            }
        }

// Add services to the container.
        builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}