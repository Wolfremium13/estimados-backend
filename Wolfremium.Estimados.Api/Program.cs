using Common.Estimation.RoomAccess.Infrastructure.Settings;
using Wolfremium.Estimados.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddRoomAccessServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.Run();

public partial class Program
{
}