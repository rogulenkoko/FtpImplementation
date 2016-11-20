
namespace FtpImplementation.FtpCommands
{
    public class ChangeDirectoryCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {
            return "250 Changed to new directory";
        }
    }
}
