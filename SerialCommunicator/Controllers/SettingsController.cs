using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers
{
    public class SettingsController : Controller
    {
        private readonly MainDbContext _dbContext;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            MainDbContext dbContext, 
            ILogger<SettingsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SettingsVm
            {
                Settings = new CommunicationSettings()
            };

            try 
            {
                if (_dbContext.CommunicationSettings.Any())
                {
                    var dbSettings = await _dbContext.CommunicationSettings
                        .FirstOrDefaultAsync(s => s.IsActive);

                    if (dbSettings != null)
                    {
                        model.Settings = dbSettings;
                        _dbContext.CommunicationSettings.Add(dbSettings);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else 
                {
                    _logger.LogInformation("No communication settings found in the database, falling back to default");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the communication settings.");
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(model);
        }

        // Maybe
        // [HttpPost]
        // public IActionResult Save(CommunicationSettings settings)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         // TODO Add message
        //         return View("Index", settings);
        //     }
        // }
    }
}
