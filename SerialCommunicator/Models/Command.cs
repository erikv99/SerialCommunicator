namespace SerialCommunicator.Models;

public class Command 
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; } 
    public required byte[] Payload { get; set; }
}