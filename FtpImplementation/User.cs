using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FtpImplementation
{
    [Serializable]
    public class User
    {
        [XmlAttribute("username")]
        public string Username { get; set; }

        [XmlAttribute("password")]
        public string Password { get; set; }

        [XmlAttribute("homedir")]
        public string HomeDir { get; set; }
    }
}
