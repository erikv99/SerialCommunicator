using System;
using System.Collections.Generic;


// Todo delete or implement properly
namespace SerialCommunicator.Extensions
{
    public static class CommandExtensions
    {
        public static bool Compare(this Command command, Command? otherCommand)
        {
            return command.Description == otherCommand?.Description &&
                   command.Name == otherCommand?.Name &&
                   command.Payload. .SequenceEqual(otherCommand?.Payload) == true;
        }

        public static void OverrideValues(this Command command, Command? otherCommand)
        {
            if (otherCommand != null)
            {
                command.Name = otherCommand.Name;
                command.Description = otherCommand.Description;
                command.Payload = otherCommand.Payload;
            }
        }
    }
}