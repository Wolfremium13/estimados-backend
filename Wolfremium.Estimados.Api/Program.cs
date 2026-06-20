using Common.Estimation.EstimationSession.Infrastructure.Settings;
using Common.Estimation.RoomAccess.Infrastructure.Settings;
using Wolfremium.Estimados.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddRoomAccessServices();
builder.Services.AddEstimationSessionServices();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (allowedOrigins is null || allowedOrigins.Length == 0)
    {
        allowedOrigins = ["http://localhost:4321", "https://estimados-front.vercel.app"];
    }

    options.AddPolicy("WebAppPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("WebAppPolicy");
app.MapHealthChecks("/health").RequireCors("WebAppPolicy");
app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.Run();

public partial class Program
{
}