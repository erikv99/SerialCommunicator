using Microsoft.AspNetCore.Mvc;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers
{
    public class SettingsController : Controller
    {
        private readonly MainDbContext _dbContext;

        public SettingsController(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var model = _dbContext.CommunicationSettings.FirstOrDefault(s => s.IsActive);
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
