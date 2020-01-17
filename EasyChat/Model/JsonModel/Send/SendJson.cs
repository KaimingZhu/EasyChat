using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Model
{
    public class Send_LoginRegisterJson
    {
        public string action;
        public string id;
        public string password;

        public Send_LoginRegisterJson(string action, string id, string password)
        {
            this.action = action;
            this.id = id;
            this.password = password;
        }
    }

    public class Send_SendMessageJson{
        public string action;
        public string from;
        public List<string> to;
        public string message;

        public Send_SendMessageJson()
        {
        }

        public Send_SendMessageJson(string action,string from,List<string> to,string message)
        {
            this.action = action;
            this.from = from;
            this.to = to;
            this.message = message;
        }
    }

    public class Send_ReadMessageJson
    {
        public string action;
        public string id;
        public Send_ReadMessageJson(string action, string id)
        {
            this.action = action;
            this.id = id;
        }
    }
}
