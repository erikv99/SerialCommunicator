namespace SerialCommunicator.Models
{
    public class CommunicatorVM
    {
        public required List<Command> Commands { get; set; }
        public required bool IsKillSwitchActive { get; set; }
        public string PromptName { get; set; } = "SerialCommunicator";
    }
}
