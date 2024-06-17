using System.IO.Ports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SerialCommunicator.Models;

namespace SerialCommunicator.Services;

public class SerialCommunicatorService
{
    private readonly MainDbContext _dbContext;
    private readonly ILogger<SerialCommunicatorService> _logger;

    public SerialCommunicatorService(
        MainDbContext dbContext,
        ILogger<SerialCommunicatorService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Sends a command through the serial port.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns>True if the command was sent successfully; otherwise, false.</returns>
    [HttpPost]
    public async Task<bool> SendCommandAsync(Command command)
    {
        _logger.LogInformation("Attempting to send the {CommandName} command.", command.Name);
        _logger.LogInformation("Payload: {Payload} command.", BitConverter.ToString(command.Payload));

        SerialPort? port = null;
        var result = false;
        var _serialPortOptions = await _loadCommunicationSettingsAsync();
        

        try
        {
            port = new SerialPort()
            {
                PortName = _serialPortOptions.PortName,
                BaudRate = _serialPortOptions.BaudRate,
                Parity = _serialPortOptions.Parity,
                DataBits = _getValidDatabits(_serialPortOptions),
                StopBits = _serialPortOptions.StopBits,
                Handshake = _serialPortOptions.Handshake,
                RtsEnable = _serialPortOptions.RtsEnable
            };

            port.Open();
            port.Write(command.Payload, offset: 0, command.Payload.Length);
            result = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while sending the '{command.Name}' command. ");
        }
        finally
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        return result;
    }

    private int _getValidDatabits(CommunicationSettings _serialPortOptions)
    {
        return Math.Clamp(_serialPortOptions.DataBits, 5, 8);
    }

    private async Task<CommunicationSettings> _loadCommunicationSettingsAsync()
    {
        var activeSettings = await _dbContext.CommunicationSettings
            .FirstOrDefaultAsync(s => s.IsActive);

        if (activeSettings == null) 
        {
            _logger.LogError("Active settings is null in DB when it should not be.");
            throw new NullReferenceException("Active settings is null in DB when it should not be.");
        }

        return activeSettings!;
    }
}