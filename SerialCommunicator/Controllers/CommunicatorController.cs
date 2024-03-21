using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers
{
    public class CommunicatorController : Controller
    {
        private readonly List<Command> _commands;
        private readonly SerialCommunicatorService _serialCommunicatorService;

        public CommunicatorController(
            IOptions<CommandOptions> commandSettings, 
            SerialCommunicatorService serialCommunicatorService) 
        {
            _commands = commandSettings.Value?.Commands ?? new List<Command>();
            _serialCommunicatorService = serialCommunicatorService;
        }

        public IActionResult Index()
        {
            var model = new CommunicatorVM 
            {
                Commands = _commands
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult RunCommand(int id) 
        {
            var command = _commands.FirstOrDefault(c => c.Id == id);

            if (command == null) 
            {
                return NotFound();
            }

            var result = _serialCommunicatorService.SendCommand(command);

            return Json(new { success = true, response = result });
        }
    }
}
