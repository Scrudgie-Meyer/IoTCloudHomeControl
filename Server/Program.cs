using Microsoft.EntityFrameworkCore;
using Server.Data.DBManager;

var builder = WebApplication.CreateBuilder(args);

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<DBSetup>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowWebApp",
//        builder => builder
//            .WithOrigins("https://localhost:7202")  // Allow your front-end URL
//            .AllowAnyMethod()
//            .AllowAnyHeader());
//});

var app = builder.Build();

//app.UseCors("AllowWebApp");

// Automatically apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DBSetup>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();