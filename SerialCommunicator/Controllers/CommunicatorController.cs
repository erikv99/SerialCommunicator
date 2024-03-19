using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers
{
    public class CommunicatorController : Controller
    {
        private readonly List<Command> _commands;

        public CommunicatorController(IOptions<CommandOptions> commandSettings) 
        {
            _commands = commandSettings.Value?.Commands ?? new List<Command>();
        }

        public IActionResult Index()
        {
            var model = new CommunicatorVM 
            {
                Commands = _commands
            };

            return View(model);
        }
    }
}
