using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyChat.Model
{
    public class Receive_LoginRegisterJson
    {
        public bool state;
        public string reason;

        public Receive_LoginRegisterJson(bool state, string reason)
        {
            this.state = state;
            this.reason = reason;
        }
    }

    public class Receive_SendJson
    {
        public bool state;
        public string reason;

        public Receive_SendJson(bool state, string reason)
        {
            this.state = state;
            this.reason = reason;
        }
    }

    public class Receive_ReadJson
    {
        public bool state;
        public List<JsonMessage> jsonMessages;

        public Receive_ReadJson()
        {
        }

        public Receive_ReadJson(bool state, List<JsonMessage> jsonMessages)
        {
            this.state = state;
            this.jsonMessages = jsonMessages;
        }
    }

    public class JsonMessage
    {
        public string from;
        public List<string> room;
        public string message;
        public string time;

        public JsonMessage()
        {

        }

        public JsonMessage(string from, List<string> room, string message, string time)
        {
            this.from = from;
            this.room = room;
            this.message = message;
            this.time = time;
        }

        public DateTime dateTime { get; internal set; }
    }
}
