using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Data.DBManager;
using Server.Data.Entities;

class Program
{
    static void Main(string[] args)
    {
        // Create a Host to manage dependencies
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Load configuration from appsettings.json
                var config = context.Configuration;
                var connectionString = config.GetConnectionString("PostgresConnection");

                // Add DB Context with PostgreSQL
                services.AddDbContext<DBSetup>(options =>
                    options.UseNpgsql(connectionString));

                services.AddTransient<App>(); // Register App class
            })
            .Build();

        // Run the application
        var app = host.Services.GetRequiredService<App>();
        app.Run();
    }
}

// Application logic class
public class App
{
    private readonly DBSetup _db;

    public App(DBSetup db)
    {
        _db = db;
    }

    public void Run()
    {
        Console.WriteLine("Database connection established!");

        // Example: Insert a user
        _db.Users.Add(new User { Username = "admin", Email = "admin@example.com", PasswordHash = "hashedpassword" });
        _db.SaveChanges();

        Console.WriteLine("User added!");
    }
}
