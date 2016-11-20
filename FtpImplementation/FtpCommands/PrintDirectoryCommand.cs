
namespace FtpImplementation.FtpCommands
{
    public class PrintDirectoryCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {
            return "257 \"E://\" is current directory.";
        }
    }
}
