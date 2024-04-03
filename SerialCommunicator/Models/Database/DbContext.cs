using Microsoft.EntityFrameworkCore;
using SerialCommunicator.Models;

public class MainDbContext : DbContext
{
    public DbSet<Command> Commands { get; set; }

    public string DbPath { get; }

    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options)
    {
        // Todo: check path 
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "MainDbContext.db");
    }
}