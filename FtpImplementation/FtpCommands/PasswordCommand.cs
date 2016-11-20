
namespace FtpImplementation.FtpCommands
{
    public class PasswordCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {
            return "230 User logged in";
        }
    }
}
