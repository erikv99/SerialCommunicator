using ElectronNET.API;
using Microsoft.EntityFrameworkCore;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

builder.Configuration
    .AddJsonFile("commands.json", optional: true, reloadOnChange: false)
    .AddJsonFile("commands.local.json", optional: true, reloadOnChange: false);

_configureServices(builder.Services);

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
    // TODO: Make this more elegant and move to own method
    services.Configure<CommandOptions>(builder.Configuration.GetSection("Container"));
    services.Configure<SerialPortOptions>(builder.Configuration.GetSection("SerialPortOptions"));

    var dataSourcePath = System.IO.Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "MainDbContext.db");

    services.AddDbContext<MainDbContext>(options =>
        options.UseSqlite($"Data Source={dataSourcePath}"), 
        ServiceLifetime.Scoped);

    services.AddControllersWithViews();
    services.AddTransient<SerialCommunicatorService>();
    services.AddSingleton<RemoteKillSwitchService>();
}