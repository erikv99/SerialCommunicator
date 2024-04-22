using System.IO.Ports;

namespace SerialCommunicator.Models;

public class CommunicationSettings
{
    public int? Id { get; set; }
    public bool IsActive { get; set; }
    public string PortName { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public int DataBits { get; set; } = 8;
    public StopBits StopBits { get; set; } = StopBits.One;
    public Handshake Handshake { get; set; } = Handshake.None;
    public int ReadTimeout { get; set; } = 500;
    public int WriteTimeout { get; set; } = 500;
}
