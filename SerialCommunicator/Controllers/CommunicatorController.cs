using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

namespace SerialCommunicator.Controllers;

public class CommunicatorController : Controller
{
    private readonly List<Command> _preConfiguredCommands;
    private readonly SerialCommunicatorService _serialCommunicatorService;
    private readonly RemoteKillSwitchService _killSwitchService;
    private readonly MainDbContext _dbContext;
    private  bool _arePreConfiguredCommandsLoaded = false;

    public CommunicatorController(
        IOptions<CommandOptions> commandSettings,
        SerialCommunicatorService serialCommunicatorService,
        RemoteKillSwitchService killSwitchService,
        MainDbContext dbContext)
    {
        // Loading the _preConfiguredCommands here and using it as a global variable seems like a bad idea.
        // TODO: Fix when time.
        _preConfiguredCommands = commandSettings.Value?.Commands ?? new List<Command>();
        _killSwitchService = killSwitchService;
        _serialCommunicatorService = serialCommunicatorService;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var isKillSwitchActive = await _killSwitchService.IsKillSwitchActive();

        if (isKillSwitchActive)
        {
            _preConfiguredCommands.Clear();
        }

        if (_shouldPreConfiguredCommandsBeConverted(isKillSwitchActive))
        {
            await _convertPreConfiguredCommandsAsync();
        }

        var model = new CommunicatorVM
        {
            IsKillSwitchActive = isKillSwitchActive,
            Commands = _preConfiguredCommands,
            PromptName = "SerialCommunicator",
        };

        return View(model);
    }

    private async Task _convertPreConfiguredCommandsAsync()
    {
        foreach (var command in _preConfiguredCommands)
        {
            var existingCommand = await _dbContext.Commands
                .FirstOrDefaultAsync(c => 
                    c.Description == command.Description 
                    && c.Name == command.Name 
                    && c.Payload.SequenceEqual(command.Payload));

            if (existingCommand == null)
            {
                _dbContext.Commands.Add(command);
            }
            else
            {
                // TODO, one day I guess.
                //existingCommand!.OverrideValues(command);
                existingCommand.Name = command.Name;
                existingCommand.Description = command.Description;
                existingCommand.Payload = command.Payload;
            }
        }

        await _dbContext.SaveChangesAsync();
        _arePreConfiguredCommandsLoaded = true;
    }

    private bool _shouldPreConfiguredCommandsBeConverted(bool isKillSwitchActive)
    {
        return !_arePreConfiguredCommandsLoaded
                    && !isKillSwitchActive
                    && _dbContext.Commands.Any();
    }

    [HttpPost]
    public IActionResult DeleteCommand(int id)
    {
        var command = _preConfiguredCommands.FirstOrDefault(c => c.Id == id);

        if (command == null)
        {
            return NotFound();
        }

        _preConfiguredCommands.Remove(command);
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult SendCommand(int id) 
    {
        var command = _preConfiguredCommands.FirstOrDefault(c => c.Id == id);

        if (command == null) 
        {
            return NotFound();
        }

        var result = _serialCommunicatorService.SendCommand(command);

        return Json(new { success = true, response = result });
    }
}
