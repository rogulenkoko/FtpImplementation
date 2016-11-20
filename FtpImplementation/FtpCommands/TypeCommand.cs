
namespace FtpImplementation.FtpCommands
{
    public class TypeCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {

            var response = "";

            string[] splitArgs = parameter.Split(' ');
            var typeCode = splitArgs[0];
            var formatControl = splitArgs.Length > 1 ? splitArgs[1] : null;

            switch (typeCode)
            {
                case "A":
                    break;
                case "I":
                    ClientConnection.TransferType = typeCode;
                    response = "200 OK";
                    break;
                //case "E":
                //case "L":
                default:
                    response = "504 Command not implemented for that parameter.";
                    break;
            }

            if (formatControl != null)
            {
                switch (formatControl)
                {
                    case "N":
                        response = "200 OK";
                        break;
                    //case "T":
                    //case "C":
                    default:
                        response = "504 Command not implemented for that parameter.";
                        break;
                }
            }

            return response;
        }
    }
}
