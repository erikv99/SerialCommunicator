using ElectronNET.API;
using SerialCommunicator.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

_configureServices(builder.Services);

builder.Configuration
    .AddJsonFile("commands.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"commands.local.json", optional: true, reloadOnChange: true);

// TODO: Make this more elegent and move to own method
builder.Services.Configure<CommandOptions>(builder.Configuration.GetSection("commands"));
builder.Services.Configure<SerialPortOptions>(builder.Configuration.GetSection("SerialPortOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// (GLOBAL) TODO add favicon

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.StartAsync();

// Temporarily disabled for development purposes.
//await Electron.WindowManager.CreateWindowAsync();

app.WaitForShutdown();

// TODO: move to extension method
void _configureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddTransient<SerialCommunicatorService>();
}