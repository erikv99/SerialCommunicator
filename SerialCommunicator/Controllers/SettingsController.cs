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
            var settings = new CommunicationSettings();

            try 
            {
                if (_dbContext.CommunicationSettings.Any())
                {
                    var dbSettings = await _dbContext.CommunicationSettings
                        .FirstOrDefaultAsync(s => s.IsActive);

                    if (dbSettings != null)
                    {
                        settings = dbSettings;
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

            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Save(CommunicationSettings model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid model state.");
                return View(model);
            }

            // Todo: improve error handling

            try
            {
                await _saveAsync(model);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while saving the communication settings.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the communication settings.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Settings saved successfully.";
            return RedirectToAction("Index");
        }

        private async Task _saveAsync(CommunicationSettings model)
        {
            var existingSettings = await _dbContext.CommunicationSettings.FindAsync(model.Id);

            if (existingSettings != null)
            {
                _dbContext.Entry(existingSettings).CurrentValues.SetValues(model);
            }
            else
            {
                _dbContext.CommunicationSettings.Add(model);
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
}
