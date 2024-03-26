using System.IO.Ports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SerialCommunicator.Models;

public class SerialCommunicatorService
{
    private readonly SerialPortOptions _serialPortOptions;
    private readonly ILogger<SerialCommunicatorService> _logger;

    public SerialCommunicatorService(
        IOptions<SerialPortOptions> serialPortOptions,
        ILogger<SerialCommunicatorService> logger)
    {
        _serialPortOptions = serialPortOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sends a command through the serial port.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns>True if the command was sent successfully; otherwise, false.</returns>
    [HttpPost]
    public bool SendCommand(Command command)
    {
        _logger.LogInformation($"Attempting to send the '{command.Name}' command.");
        SerialPort? port = null;
        var result = false;

        try
        {
            port = new SerialPort()
            {
                PortName = _serialPortOptions.PortName,
                BaudRate = _serialPortOptions.BaudRate,
                Parity = _serialPortOptions.Parity,
                DataBits = _serialPortOptions.DataBits,
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
}