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
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("WebAppPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:4321")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

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
app.UseHealthChecks("/health");
app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.Run();

public partial class Program
{
}