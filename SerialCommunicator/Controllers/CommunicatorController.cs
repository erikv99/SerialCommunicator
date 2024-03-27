using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;

namespace SerialCommunicator.Controllers;

public class CommunicatorController : Controller
{
    private readonly List<Command> _commands;
    private readonly SerialCommunicatorService _serialCommunicatorService;

    public CommunicatorController(
        IOptions<CommandOptions> commandSettings,
        SerialCommunicatorService serialCommunicatorService)
    {
        _commands = commandSettings.Value?.Commands ?? _createMockCommands();// new List<Command>();
        _serialCommunicatorService = serialCommunicatorService;
    }

    private List<Command> _createMockCommands()
    {
        return new List<Command>
        {
            new Command
            {
                Id = 1,
                Name = "Command 1",
                Description = "This command initiates the first nothing sequence in the system. It's primarily used to set up nothing and prepare the system for absolutely nothing.",
                Payload =
                [
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08
                ]
            },
            new Command
            {
                Id = 2,
                Name = "Command 2",
                Description = "Description for Command 2",
                Payload =
                [
                    0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
                ]
            },
            new Command
            {
                Id = 3,
                Name = "Command 3",
                Description = "Description for Command 3",
                Payload =
                [
                    0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18
                ]
            }
        };
    }

    public IActionResult Index()
    {
        var model = new CommunicatorVM 
        {
            Commands = _commands,
            PromptName = "SerialCommunicator"
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult SendCommand(int id) 
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
