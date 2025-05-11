using Microsoft.EntityFrameworkCore;
using Server.Data.DBManager;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Host=iotcloudhomecontrol.cly2e42068ya.eu-north-1.rds.amazonaws.com;Port=5432;Database=IOTDB;Username=postgres;Password=w7cpguM4yJ5y2Mq";

builder.Services.AddDbContext<DBSetup>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

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

app.MapGet("/", () => "Server is running");

app.Run();