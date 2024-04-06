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
    private readonly ILogger<CommunicatorController> _logger;
    private  bool _arePreConfiguredCommandsLoaded = false;

    public CommunicatorController(
        IOptions<CommandOptions> commandSettings,
        SerialCommunicatorService serialCommunicatorService,
        RemoteKillSwitchService killSwitchService,
        MainDbContext dbContext,
        ILogger<CommunicatorController> logger)
    {
        // Loading the _preConfiguredCommands here and using it as a global variable seems like a bad idea.
        // TODO: Fix when time.
        _preConfiguredCommands = commandSettings.Value?.Commands ?? new List<Command>();
        _killSwitchService = killSwitchService;
        _serialCommunicatorService = serialCommunicatorService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var isKillSwitchActive = await _killSwitchService.IsKillSwitchActive();

        if (isKillSwitchActive)
        {
            _preConfiguredCommands.Clear();
        }

        // TODO change the way we use _preConfiguredCommands, it's not a good idea to use it as a global variable.
        if (_shouldPreConfiguredCommandsBeConverted(isKillSwitchActive))
        {
            await _convertPreConfiguredCommandsAsync();
        }
        
        var model = new CommunicatorVM
        {
            IsKillSwitchActive = isKillSwitchActive,
            Commands = await _loadCommandsAsync(_preConfiguredCommands),
            PromptName = "SerialCommunicator",
        };

        return View(model);
    }

    private async Task<List<Command>> _loadCommandsAsync(List<Command> commands)
    {
        var commandsFromDb = await _dbContext.Commands.ToListAsync();
        commands.AddRange(commandsFromDb);
        return commands;
    }

    private async Task _convertPreConfiguredCommandsAsync()
    {
        foreach (var command in _preConfiguredCommands)
        {
            var existingCommand = await _tryLoadExistingCommandAsync(command);

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

    private async Task<Command?> _tryLoadExistingCommandAsync(Command command)
    {
        return await _dbContext.Commands
            .FirstOrDefaultAsync(c =>
                c.Description == command.Description
                && c.Name == command.Name
                && c.Payload.SequenceEqual(command.Payload));
    }

    private bool _shouldPreConfiguredCommandsBeConverted(bool isKillSwitchActive)
    {
        return !_arePreConfiguredCommandsLoaded
                    && !isKillSwitchActive
                    && _dbContext.Commands.Any();
    }

    private byte[] _extractPayload(string payload)
    {
        string[] hexValues = payload.Split('-');
        return hexValues
            .Select(hex => Convert.ToByte(hex, 16))
            .ToArray();
    }

    [HttpGet]
    public async Task<IActionResult> GetCommands()
    {
        var commands = await _dbContext.Commands.ToListAsync();
        commands = commands.OrderByDescending(c => c.Id).ToList();
        return PartialView("_CommandsOverviewPartial", commands);
    }

    [HttpPost]
    public async Task<IActionResult> AddCommand(string name, string description, string payload) 
    {
        try
        {
            var command = new Command
            {
                Name = name,
                Description = description,
                Payload = _extractPayload(payload) // Todo rename to rawPayload, also to do on the frontend post call.
            };

            await _dbContext.Commands.AddAsync(command);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a command.");
            return BadRequest();
        }
    }

    // Todo: this design choice feels odd, change if needed.

    [HttpPost]
    public async Task<IActionResult> DeleteCommand(int id)
    {
        var command = await _dbContext.Commands.FirstOrDefaultAsync(c => c.Id == id);

        if (command == null)
        {
            return NotFound();
        }

        _dbContext.Commands.Remove(command);
        await _dbContext.SaveChangesAsync();

        return Ok();
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
