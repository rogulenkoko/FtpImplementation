
namespace FtpImplementation.FtpCommands
{
    public class UserCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {
            return "331 Username ok, need password";
        }
    }
}
