using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

namespace SerialCommunicator.Controllers;

public class CommunicatorController : Controller
{
    private readonly List<Command> _commands;
    private readonly SerialCommunicatorService _serialCommunicatorService;
    private readonly RemoteKillSwitchService _killSwitchService;

    public CommunicatorController(
        IOptions<CommandOptions> commandSettings,
        SerialCommunicatorService serialCommunicatorService,
        RemoteKillSwitchService killSwitchService)
    {
        _commands = commandSettings.Value?.Commands ?? new List<Command>();
        _killSwitchService = killSwitchService;
        _serialCommunicatorService = serialCommunicatorService;
    }

    public async Task<IActionResult> Index()
    {
        var isKillSwitchActive = await _killSwitchService.IsKillSwitchActive();

        if (isKillSwitchActive)
        {
            _commands.Clear();
        }

        var model = new CommunicatorVM
        {
            IsKillSwitchActive = isKillSwitchActive,
            Commands = _commands,
            PromptName = "SerialCommunicator",
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
