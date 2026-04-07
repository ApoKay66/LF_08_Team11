using System.Net;
using System.Net.Sockets;
using BrainBusters.DataAccess;
using BrainBuster.Web.Hubs;
using BrainBuster.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services Configuration ---

builder.Services.AddControllers(); // For Web API
builder.Services.AddSignalR();     // For real-time updates

// Register custom services
builder.Services.AddSingleton<QuizDatabase>(_ => new QuizDatabase("../BrainBuster/Data/quiz.db"));
builder.Services.AddSingleton<GameHubService>(); // The central, shared game state

// Make the web app accessible on the local network
var port = 5000;
builder.WebHost.UseUrls($"http://*:{port}");


var app = builder.Build();

// --- HTTP Request Pipeline Configuration ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles(); // Serve index.html for root path
app.UseStaticFiles();  // Serve files from wwwroot

app.UseRouting();

app.UseAuthorization();

// Map controllers and hubs
app.MapControllers(); // Maps API controllers
app.MapHub<GameHub>("/gameHub"); // Maps the SignalR hub

// --- Display Local IP and Run ---

var host = Dns.GetHostEntry(Dns.GetHostName());
var ipAddresses = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

Console.WriteLine("====================================================");
Console.WriteLine("Brain Buster Web App Started!");
if (ipAddresses.Any())
{
    Console.WriteLine("Access it on your local network:");
    foreach (var ip in ipAddresses)
    {
        Console.WriteLine($"  http://{ip}:{port}");
    }
}
Console.WriteLine($"Or on this machine: http://localhost:{port}");
Console.WriteLine("====================================================");


app.Run();
