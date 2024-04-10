using System.Text;

namespace SerialCommunicator.Models;

public class Command 
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; } 
    public required byte[] Payload { get; set; }

    public string GetPayloadAsString()
    {
        StringBuilder builder = new(Payload.Length * 3);

        foreach (byte b in Payload)
        {
            builder.AppendFormat("0x{0:x2} ", b); 
        }
        
        return builder.ToString().Trim();
    }
}