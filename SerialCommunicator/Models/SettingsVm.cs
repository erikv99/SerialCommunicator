namespace SerialCommunicator.Models;

public class SettingsVm
{
    public string Message { get; set; } = string.Empty;
    public required CommunicationSettings Settings { get; set; }
}