
namespace FtpImplementation.FtpCommands
{
    public class QuitCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }
        public string Execute(string parameter)
        {
            return "221 Service closing control connection";
        }
    }
}
