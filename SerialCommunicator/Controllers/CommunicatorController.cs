using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;
using SerialCommunicator.Services;

namespace SerialCommunicator.Controllers;

public class CommunicatorController : Controller
{
    private readonly List<Command>? _preConfiguredCommands;
    private readonly SerialCommunicatorService _serialCommunicatorService;
    private readonly RemoteKillSwitchService _killSwitchService;
    private readonly MainDbContext _dbContext;
    private readonly ILogger<CommunicatorController> _logger;
    
    private bool _isKillSwitchActive = false;

    public CommunicatorController(
        IOptions<CommandOptions> commandOptions,
        SerialCommunicatorService serialCommunicatorService,
        RemoteKillSwitchService killSwitchService,
        MainDbContext dbContext,
        ILogger<CommunicatorController> logger)
    {
        _preConfiguredCommands = commandOptions.Value?.Commands;
        _killSwitchService = killSwitchService;
        _serialCommunicatorService = serialCommunicatorService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // TODO Change this to a redirect to a different page if the kill switch is active.
        _isKillSwitchActive = await _killSwitchService.IsKillSwitchActive();

        var model = new CommunicatorVM
        {
            IsKillSwitchActive = _isKillSwitchActive,
            Commands = await _loadCommandsAsync(),
            PromptName = "SerialCommunicator",
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetCommands()
    {
        if (_isKillSwitchActive)
        {
            return BadRequest();
        }

        var commands = await _dbContext.Commands.ToListAsync();
        commands = commands.OrderByDescending(c => c.Id).ToList();
        return PartialView("_CommandsOverviewPartial", commands);
    }

    // TODO move actual command logic to a service.
    [HttpPost]
    public async Task<IActionResult> AddCommand(string name, string description, string rawPayload) 
    {
        if (_isKillSwitchActive)
        {
            return BadRequest();
        }

        try
        {
            var command = new Command
            {
                Name = name,
                Description = description,
                Payload = _extractPayload(rawPayload)
            };

            await _dbContext.Commands.AddAsync(command);
            await _dbContext.SaveChangesAsync();

            return Ok();
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
        if (_isKillSwitchActive)
        {
            return BadRequest();
        }

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
        if (_isKillSwitchActive)
        {
            return BadRequest();
        }

        var command = _dbContext.Commands.FirstOrDefault(c => c.Id == id);

        if (command == null) 
        {
            return NotFound();
        }

        var result = _serialCommunicatorService.SendCommand(command);

        if (!result)
        {
            return BadRequest();
        }

        return Ok();
    }

    private async Task<List<Command>> _loadCommandsAsync()
    {
        var commands = new List<Command>();

        if (_isKillSwitchActive)
        {
            return commands;
        }

        if (_preConfiguredCommands != null && _preConfiguredCommands.Any())
        {
            commands.AddRange(_preConfiguredCommands);
        }

        var commandsFromDb = await _dbContext.Commands.ToListAsync();
        if (commandsFromDb != null && commands.Any())
        {
            commands.AddRange(commandsFromDb);
        }

        return commands;
    }

    private byte[] _extractPayload(string payload)
    {
        string[] hexValues = payload.Split('-');
        return hexValues
            .Select(hex => Convert.ToByte(hex, 16))
            .ToArray();
    }
}
