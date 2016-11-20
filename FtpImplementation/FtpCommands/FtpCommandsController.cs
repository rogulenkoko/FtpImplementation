using System;
using System.Collections.Generic;

namespace FtpImplementation.FtpCommands
{
    public class FtpCommandsController
    {
        private readonly Dictionary<string,ICommand> _commands = new Dictionary<string, ICommand>();

        private static Lazy<FtpCommandsController> InstanseFactory = new Lazy<FtpCommandsController>(()=>new FtpCommandsController()); 

        public static FtpCommandsController Instance { get { return InstanseFactory.Value; } }

        private FtpCommandsController()
        {
            _commands.Add("USER",new UserCommand());
            _commands.Add("PASS",new PasswordCommand());

            var changedirectoryCommand = new ChangeDirectoryCommand();
            _commands.Add("CWD",changedirectoryCommand);
            _commands.Add("CDUP",changedirectoryCommand);

            _commands.Add("TYPE",new TypeCommand());
            _commands.Add("PORT",new PortCommand());
            _commands.Add("PASV",new PassiveCommand());
            _commands.Add("LIST",new ListCommand());
            _commands.Add("RETR",new RetrieveCommand());
            _commands.Add("QUIT",new QuitCommand());
        }

        public ICommand GetCommand(string command, ClientConnection connection)
        {
            ICommand commandResult = null;
            if (_commands.ContainsKey(command))
            {
                commandResult = _commands[command];
                commandResult.ClientConnection = connection;
            }
            return commandResult;
        }
    }
}
