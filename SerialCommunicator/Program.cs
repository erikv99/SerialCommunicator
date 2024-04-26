using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

builder.Configuration
    .AddJsonFile("commands.json", optional: true, reloadOnChange: false)
    .AddJsonFile("commands.local.json", optional: true, reloadOnChange: false);

_configureServices(builder.Services);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else 
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.StartAsync();

await _configureDatabaseAsync(app);

app.WaitForShutdown();

async Task ElectronBootstrapAsync()
{
    BrowserWindowOptions options = new BrowserWindowOptions
    {
        Show = false,
        WebPreferences = new WebPreferences
        {
            NodeIntegration = true,
        },
    };

    BrowserWindow mainWindow = await Electron.WindowManager.CreateWindowAsync(options);

    mainWindow.OnClosed += Electron.App.Quit;

    mainWindow.OnReadyToShow += () =>
    {
        mainWindow.Show();
        mainWindow.SetTitle("SerialCommunicator");

        if (app == null) 
        {
            Console.WriteLine("App is null.");
           return;
        }

        if (app.Environment.IsDevelopment())
        {
            mainWindow.WebContents.OpenDevTools();
        }
    };
}

void _configureServices(IServiceCollection services)
{
    var section = builder.Configuration.GetSection("Container");
    services.Configure<CommandOptions>(section);

    var dataSourcePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MainDbContext.db");

    services.AddDbContext<MainDbContext>(options =>
        options.UseSqlite($"Data Source={dataSourcePath}"),
        ServiceLifetime.Scoped);

    services.AddControllersWithViews();
    services.AddTransient<SerialCommunicatorService>();
    services.AddSingleton<RemoteKillSwitchService>();

    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });
}

async Task _configureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Configuring the database...");

    await dbContext.Database.MigrateAsync();

    if (!dbContext.CommunicationSettings.Any())
    {
        dbContext.CommunicationSettings.Add(new CommunicationSettings());
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Communication settings added to the database.");
    }

    if (!dbContext.Commands.Any())
    {
        var commandOptions = scope.ServiceProvider.GetRequiredService<IOptions<CommandOptions>>().Value;

        if (commandOptions?.Commands != null)
        {
            dbContext.Commands.AddRange(entities: commandOptions.Commands);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Commands added to the database.");
        }
        else
        {
            logger.LogWarning("No commands found in the configuration.");
        }
    }
}