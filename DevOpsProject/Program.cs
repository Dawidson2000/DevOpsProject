using DevOpsProject;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = "Server=(localdb)\\mssqllocaldb;Database=DevOpsDb;Trusted_Connection=True;";
if (!builder.Environment.IsDevelopment())
{
    connectionString = "Server=db;Database=WeatherDb;User=sa;Password=YourStrong!Password;TrustServerCertificate=True;";
}
builder.Services.AddDbContext<WeatherForecastContext>(options => options.UseSqlServer(connectionString));

builder.WebHost.UseUrls("http://0.0.0.0:8080", "http://0.0.0.0:80");

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WeatherForecastContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching", "Foggy", "Windy"
};

// API endpoint to add sample forecasts to the database
app.MapPost("/weatherforecast", async (WeatherForecastContext db) =>
{
    var forecasts = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            new Guid(),
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    db.WeatherForecasts.AddRange(forecasts);
    await db.SaveChangesAsync();

    return Results.Ok(forecasts);
})
.WithName("CreateWeatherForecast")
.WithOpenApi();

// API endpoint to fetch forecasts from the database
app.MapGet("/weatherforecast", async (WeatherForecastContext db) =>
    await db.WeatherForecasts.ToListAsync())
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
