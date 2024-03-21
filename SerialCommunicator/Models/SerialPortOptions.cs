using System.IO.Ports;

namespace SerialCommunicator.Models;

public class SerialPortOptions
{
    public required string PortName { get; set; }
    public required int BaudRate { get; set; } = 9600;
    public Parity Parity { get; set; } = Parity.None;
    public int DataBits { get; set; } = 8;
    public StopBits StopBits { get; set; } = StopBits.One;
    public Handshake Handshake { get; set; } = Handshake.None;
    public bool RtsEnable { get; set; } = false;
}