using ElectronNET.API;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// TODO add favicon

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

TODO TODO TODO TODO 
        Configuration = configuration;

        List<Command> commandSequences;
        try
        {
            string commandSequencesJson;
            if (File.Exists("CommandSequences.local.json"))
            {
                commandSequencesJson = File.ReadAllText("CommandSequences.local.json");
            }
            else
            {
                commandSequencesJson = File.ReadAllText("CommandSequences.json");
            }

            commandSequences = JsonSerializer.Deserialize<List<Command>>(commandSequencesJson);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            // Handle file not found exception
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            // Handle JSON parsing exception
        }
    }

    public IConfiguration Configuration { get; }