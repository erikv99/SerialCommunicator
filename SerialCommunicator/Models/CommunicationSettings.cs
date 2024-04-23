using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace SerialCommunicator.Models;

public class CommunicationSettings
{
    public int? Id { get; set; }

    public bool IsActive { get; set; } = true;

    [DisplayName("Port Name")]
    [Required(ErrorMessage = "Port Name is required.")]
    public string PortName { get; set; } = "COM1";

    [DisplayName("Baud Rate")]
    [Range(1, int.MaxValue, ErrorMessage = "Baud Rate must be a positive number.")]
    public int BaudRate { get; set; } = 9600;

    [DisplayName("Parity")]
    public Parity Parity { get; set; } = Parity.None;

    [DisplayName("Data Bits")]
    [Range(5, 8, ErrorMessage = "Data Bits must be between 5 and 8.")]
    public int DataBits { get; set; } = 8;

    [DisplayName("Stop Bits")]
    public StopBits StopBits { get; set; } = StopBits.One;

    [DisplayName("Handshake")]
    public Handshake Handshake { get; set; } = Handshake.None;

    [DisplayName("Read Timeout")]
    [Range(0, int.MaxValue, ErrorMessage = "Read Timeout must be a non-negative number.")]
    public int ReadTimeout { get; set; } = 500;

    [DisplayName("Write Timeout")]
    [Range(0, int.MaxValue, ErrorMessage = "Write Timeout must be a non-negative number.")]
    public int WriteTimeout { get; set; } = 500;

    [DisplayName("RTS Enable")]
    public bool RtsEnable { get; set; } = false;
}
