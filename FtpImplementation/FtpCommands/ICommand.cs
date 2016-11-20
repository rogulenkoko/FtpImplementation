namespace FtpImplementation.FtpCommands
{
    public interface ICommand
    {
        ClientConnection ClientConnection { get; set; }
        string Execute(string parameter);
    }
}
