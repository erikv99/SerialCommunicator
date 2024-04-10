using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("commands.json", optional: true, reloadOnChange: false)
    .AddJsonFile("commands.local.json", optional: true, reloadOnChange: false);

_configureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.StartAsync();

try
{
    await _configureDatabaseAsync(app);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "An error occurred: {Message}", ex.Message);
}

app.WaitForShutdown();

void _configureServices(IServiceCollection services)
{
    services.Configure<CommandOptions>(builder.Configuration.GetSection("Container"));
    services.Configure<SerialPortOptions>(builder.Configuration.GetSection("SerialPortOptions"));

    var dataSourcePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MainDbContext.db");

    services.AddDbContext<MainDbContext>(options =>
        options.UseSqlite($"Data Source={dataSourcePath}"),
        ServiceLifetime.Scoped);

    services.AddControllersWithViews();
    services.AddTransient<SerialCommunicatorService>();
    services.AddSingleton<RemoteKillSwitchService>();

    // Add logging
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });
}

/// <summary>
/// Configures the database for the web application.
/// </summary>
/// <param name="app">The web application.</param>
/// <returns>A task representing the asynchronous operation.</returns>
async Task _configureDatabaseAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<MainDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Configuring the database...");

        await dbContext.Database.MigrateAsync();

        if (!dbContext.Commands.Any())
        {
            var commandOptions = serviceProvider.GetRequiredService<IOptions<CommandOptions>>().Value;

            if (commandOptions?.Commands != null)
            {
                dbContext.Commands.AddRange(entities: commandOptions.Commands);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Commands added to the database.");
            }
        }
    }
}
